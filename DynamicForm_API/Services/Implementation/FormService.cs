using AutoMapper;
using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Forms;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Shared.Enums;
using DynamicForm_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Services.Implementation
{
    public class FormService : IFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FormService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FormDetailResponseDto> CreateFormAsync(CreateFormDraftRequestDto request, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Forms.ExistsByCodeAsync(request.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Form code '{request.Code}' already exists.");
            }

            var utcNow = DateTime.UtcNow;
            var form = new Form
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                Versions =
                [
                    new FormVersion
                    {
                        VersionNumber = 1,
                        Status = FormVersionStatus.Draft,
                        IsCurrent = false,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    }
                ]
            };

            await _unitOfWork.Forms.AddAsync(form, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<FormDetailResponseDto>(form);
        }

        public async Task<FormDetailResponseDto> UpdateFormMetadataAsync(int formId, UpdateFormMetadataRequestDto request, CancellationToken cancellationToken = default)
        {
            var form = await GetFormByIdWithVersionsAsync(formId, cancellationToken);

            form.Name = request.Name;
            form.Description = request.Description;
            form.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Forms.Update(form);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<FormDetailResponseDto>(form);
        }

        public async Task<FormDetailResponseDto> ArchiveFormAsync(int formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormByIdWithVersionsAsync(formId, cancellationToken);

            var utcNow = DateTime.UtcNow;
            foreach (var version in form.Versions)
            {
                version.Status = FormVersionStatus.Archived;
                version.IsCurrent = false;
                version.UpdatedAt = utcNow;
            }

            form.UpdatedAt = utcNow;
            _unitOfWork.Forms.Update(form);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<FormDetailResponseDto>(form);
        }

        public async Task<PagedResponseDto<FormSummaryResponseDto>> GetFormsAsync(PagedRequestDto request, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.Forms.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _unitOfWork.Forms.GetQueryable().CountAsync(cancellationToken);

            return new PagedResponseDto<FormSummaryResponseDto>
            {
                Items = _mapper.Map<IReadOnlyList<FormSummaryResponseDto>>(items),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<FormDetailResponseDto> GetFormByIdAsync(int formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormByIdWithVersionsAsync(formId, cancellationToken);
            return _mapper.Map<FormDetailResponseDto>(form);
        }

        public async Task<FormWithCurrentPublishedVersionResponseDto> GetCurrentPublishedFormAsync(string formCode, CancellationToken cancellationToken = default)
        {
            var form = await _unitOfWork.Forms.GetByCodeAsync(formCode, cancellationToken)
                ?? throw new KeyNotFoundException($"Form with code '{formCode}' was not found.");

            var publishedVersion = await ResolveCurrentPublishedVersionAsync(form.Id, cancellationToken);

            return new FormWithCurrentPublishedVersionResponseDto
            {
                FormId = form.Id,
                FormName = form.Name,
                FormCode = form.Code,
                CurrentPublishedVersion = _mapper.Map<FormVersionDetailResponseDto>(publishedVersion)
            };
        }

        private async Task<Form> GetFormByIdWithVersionsAsync(int formId, CancellationToken cancellationToken)
        {
            var form = await _unitOfWork.Forms.GetQueryable()
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == formId, cancellationToken);

            if (form is null)
            {
                throw new KeyNotFoundException($"Form with id '{formId}' was not found.");
            }

            return form;
        }

        private async Task<FormVersion> ResolveCurrentPublishedVersionAsync(int formId, CancellationToken cancellationToken)
        {
            var version = await _unitOfWork.FormVersions.GetCurrentPublishedVersionAsync(formId, cancellationToken);
            if (version is null)
            {
                throw new InvalidOperationException($"No current published version exists for form '{formId}'.");
            }

            return version;
        }
    }
}

