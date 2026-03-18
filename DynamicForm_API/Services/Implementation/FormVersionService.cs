using AutoMapper;
using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Services.Interfaces;
using DynamicForm_API.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Services.Implementation
{
    public class FormVersionService : IFormVersionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FormVersionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FormVersionDetailResponseDto> CreateNextDraftVersionAsync(int formId, CreateNextDraftVersionRequestDto request, CancellationToken cancellationToken = default)
        {
            await EnsureFormExistsAsync(formId, cancellationToken);

            FormVersion? sourceVersion = null;
            if (request.SourceVersionId.HasValue)
            {
                sourceVersion = await LoadVersionOrThrowAsync(formId, request.SourceVersionId.Value, cancellationToken);
            }

            var nextVersionNumber = await _unitOfWork.FormVersions.GetNextVersionNumberAsync(formId, cancellationToken);
            var utcNow = DateTime.UtcNow;
            var version = new FormVersion
            {
                FormId = formId,
                VersionNumber = nextVersionNumber,
                Status = FormVersionStatus.Draft,
                IsCurrent = false,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            if (sourceVersion is not null)
            {
                version.Fields = sourceVersion.Fields
                    .OrderBy(f => f.Order)
                    .Select(f => new FormField
                    {
                        Type = f.Type,
                        Label = f.Label,
                        Name = f.Name,
                        IsRequired = f.IsRequired,
                        Order = f.Order,
                        GroupKey = f.GroupKey,
                        BindingKey = f.BindingKey,
                        RegexPattern = f.RegexPattern,
                        ConfigJson = f.ConfigJson,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    })
                    .ToList();
            }

            await _unitOfWork.FormVersions.AddAsync(version, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapVersionDetailAsync(version, cancellationToken);
        }

        public async Task<FormVersionDetailResponseDto> UpdateDraftVersionAsync(int formId, int versionId, UpdateDraftVersionRequestDto request, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);

            version.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapVersionDetailAsync(version, cancellationToken);
        }

        public async Task<FormVersionDetailResponseDto> PublishDraftVersionAsync(int formId, int versionId, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var currentPublished = await _unitOfWork.FormVersions.GetCurrentPublishedVersionAsync(formId, cancellationToken);
                if (currentPublished is not null)
                {
                    currentPublished.IsCurrent = false;
                    currentPublished.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.FormVersions.Update(currentPublished);
                }

                version.Status = FormVersionStatus.Published;
                version.IsCurrent = true;
                version.PublishedAt = DateTime.UtcNow;
                version.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.FormVersions.Update(version);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return await MapVersionDetailAsync(version, cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<FormVersionDetailResponseDto> ArchiveVersionAsync(int formId, int versionId, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);

            version.Status = FormVersionStatus.Archived;
            version.IsCurrent = false;
            version.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapVersionDetailAsync(version, cancellationToken);
        }

        public async Task<PagedResponseDto<FormVersionHistoryItemResponseDto>> GetVersionsByFormIdAsync(int formId, PagedRequestDto request, CancellationToken cancellationToken = default)
        {
            await EnsureFormExistsAsync(formId, cancellationToken);

            var query = _unitOfWork.FormVersions.GetQueryable()
                .Where(x => x.FormId == formId)
                .OrderByDescending(x => x.VersionNumber)
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponseDto<FormVersionHistoryItemResponseDto>
            {
                Items = _mapper.Map<IReadOnlyList<FormVersionHistoryItemResponseDto>>(items),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<FormVersionDetailResponseDto> GetVersionDetailsAsync(int formId, int versionId, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            return await MapVersionDetailAsync(version, cancellationToken);
        }

        public async Task<FormFieldResponseDto> AddDraftFieldAsync(int formId, int versionId, AddDraftFieldRequestDto request, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);
            ValidateFieldUniqueness(version.Fields, request.Name, request.Order);

            var utcNow = DateTime.UtcNow;
            var field = new FormField
            {
                Type = request.Type,
                Label = request.Label,
                Name = request.Name,
                IsRequired = request.IsRequired,
                Order = request.Order,
                GroupKey = request.GroupKey,
                BindingKey = request.BindingKey,
                RegexPattern = request.RegexPattern,
                ConfigJson = request.ConfigJson,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            version.Fields.Add(field);
            version.UpdatedAt = utcNow;
            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapFieldResponseAsync(field, cancellationToken);
        }

        public async Task<FormFieldResponseDto> UpdateDraftFieldAsync(int formId, int versionId, int fieldId, UpdateDraftFieldRequestDto request, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);

            var field = version.Fields.FirstOrDefault(x => x.Id == fieldId)
                ?? throw new KeyNotFoundException($"Field with id '{fieldId}' was not found in version '{versionId}'.");

            if (version.Fields.Any(x => x.Id != fieldId && x.Order == request.Order))
            {
                throw new InvalidOperationException($"Field order '{request.Order}' already exists in version '{versionId}'.");
            }

            field.Label = request.Label;
            field.IsRequired = request.IsRequired;
            field.Order = request.Order;
            field.GroupKey = request.GroupKey;
            field.BindingKey = request.BindingKey;
            field.RegexPattern = request.RegexPattern;
            field.ConfigJson = request.ConfigJson;
            field.UpdatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapFieldResponseAsync(field, cancellationToken);
        }

        public async Task<bool> RemoveDraftFieldAsync(int formId, int versionId, int fieldId, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);

            var field = version.Fields.FirstOrDefault(x => x.Id == fieldId)
                ?? throw new KeyNotFoundException($"Field with id '{fieldId}' was not found in version '{versionId}'.");

            version.Fields.Remove(field);
            version.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<IReadOnlyList<FormFieldResponseDto>> ReorderDraftFieldsAsync(int formId, int versionId, ReorderDraftFieldsRequestDto request, CancellationToken cancellationToken = default)
        {
            var version = await LoadVersionOrThrowAsync(formId, versionId, cancellationToken);
            EnsureDraftOnlyModification(version);

            var fieldIds = version.Fields.Select(x => x.Id).ToHashSet();
            var requestIds = request.Items.Select(x => x.FieldId).ToHashSet();

            if (!fieldIds.SetEquals(requestIds))
            {
                throw new InvalidOperationException("Reorder request must include all draft fields exactly once.");
            }

            foreach (var item in request.Items)
            {
                var field = version.Fields.First(x => x.Id == item.FieldId);
                field.Order = item.Order;
                field.UpdatedAt = DateTime.UtcNow;
            }

            version.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.FormVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var ordered = version.Fields.OrderBy(x => x.Order).ToList();
            var fieldDtos = new List<FormFieldResponseDto>(ordered.Count);
            foreach (var field in ordered)
            {
                fieldDtos.Add(await MapFieldResponseAsync(field, cancellationToken));
            }

            return fieldDtos;
        }

        private async Task EnsureFormExistsAsync(int formId, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.Forms.GetQueryable()
                .AsNoTracking()
                .AnyAsync(x => x.Id == formId, cancellationToken);

            if (!exists)
            {
                throw new KeyNotFoundException($"Form with id '{formId}' was not found.");
            }
        }

        private async Task<FormVersion> LoadVersionOrThrowAsync(int formId, int versionId, CancellationToken cancellationToken)
        {
            var version = await _unitOfWork.FormVersions.GetQueryable()
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.Id == versionId && x.FormId == formId, cancellationToken);

            if (version is null)
            {
                throw new KeyNotFoundException($"Version with id '{versionId}' was not found for form '{formId}'.");
            }

            return version;
        }

        private static void EnsureDraftOnlyModification(FormVersion version)
        {
            if (version.Status != FormVersionStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Version '{version.Id}' is not draft. Published/archived versions are immutable.");
            }
        }

        private static void ValidateFieldUniqueness(IEnumerable<FormField> fields, string fieldName, int fieldOrder)
        {
            if (fields.Any(x => x.Name == fieldName))
            {
                throw new InvalidOperationException($"Field name '{fieldName}' already exists in this version.");
            }

            if (fields.Any(x => x.Order == fieldOrder))
            {
                throw new InvalidOperationException($"Field order '{fieldOrder}' already exists in this version.");
            }
        }

        private async Task<FormVersionDetailResponseDto> MapVersionDetailAsync(FormVersion version, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<FormVersionDetailResponseDto>(version);
            dto.Fields = dto.Fields.OrderBy(x => x.Order).ToList();
            var fieldMap = version.Fields.ToDictionary(x => x.Id);
            await EnrichFieldDtosAsync(dto.Fields, fieldMap, cancellationToken);
            return dto;
        }

        private async Task<FormFieldResponseDto> MapFieldResponseAsync(FormField field, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<FormFieldResponseDto>(field);
            await EnrichFieldDtosAsync([dto], new Dictionary<int, FormField> { [field.Id] = field }, cancellationToken);
            return dto;
        }

        private async Task EnrichFieldDtosAsync(
            IReadOnlyList<FormFieldResponseDto> fieldDtos,
            Dictionary<int, FormField> sourceFieldMap,
            CancellationToken cancellationToken)
        {
            var lookupCache = new Dictionary<string, IReadOnlyList<FormFieldOptionResponseDto>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fieldDto in fieldDtos)
            {
                if (!sourceFieldMap.TryGetValue(fieldDto.Id, out var sourceField))
                {
                    continue;
                }

                var parsedConfig = FieldConfigHelper.Parse(sourceField.ConfigJson);
                fieldDto.OptionsMode = parsedConfig.OptionsMode;
                fieldDto.LookupKey = parsedConfig.LookupKey;
                fieldDto.ActiveOnly = parsedConfig.ActiveOnly;

                if (parsedConfig.OptionsMode == OptionsMode.Static && parsedConfig.StaticOptions.Count > 0)
                {
                    fieldDto.Options = parsedConfig.StaticOptions
                        .Select(x => new FormFieldOptionResponseDto
                        {
                            Value = x.Value,
                            Label = x.Label,
                            Order = x.Order,
                            ParentValue = x.ParentValue
                        })
                        .OrderBy(x => x.Order)
                        .ToList();

                    continue;
                }

                if (parsedConfig.OptionsMode == OptionsMode.Lookup && !string.IsNullOrWhiteSpace(parsedConfig.LookupKey))
                {
                    if (!lookupCache.TryGetValue(parsedConfig.LookupKey, out var options))
                    {
                        options = await LoadLookupOptionsAsync(parsedConfig.LookupKey, parsedConfig.ActiveOnly, cancellationToken);
                        lookupCache[parsedConfig.LookupKey] = options;
                    }

                    fieldDto.Options = options;
                }
            }
        }

        private async Task<IReadOnlyList<FormFieldOptionResponseDto>> LoadLookupOptionsAsync(string lookupCode, bool activeOnly, CancellationToken cancellationToken)
        {
            var lookup = await _unitOfWork.Lookups.GetByCodeAsync(lookupCode, cancellationToken);
            if (lookup is null || lookup.IsArchived)
            {
                return Array.Empty<FormFieldOptionResponseDto>();
            }

            var query = lookup.Items.AsEnumerable();
            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            return query
                .OrderBy(x => x.Order)
                .Select(x => new FormFieldOptionResponseDto
                {
                    Value = x.Value,
                    Label = x.Label,
                    Order = x.Order,
                    ParentValue = x.ParentValue
                })
                .ToList();
        }
    }
}
