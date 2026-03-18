using AutoMapper;
using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Lookups;
using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Services.Implementation
{
    public class LookupService : ILookupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LookupService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<LookupTableDetailResponseDto> CreateLookupTableAsync(CreateLookupTableRequestDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Lookups.GetByCodeAsync(request.Code, cancellationToken);
            if (existing is not null)
            {
                throw new InvalidOperationException($"Lookup table code '{request.Code}' already exists.");
            }

            var utcNow = DateTime.UtcNow;
            var lookup = new LookupTable
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                IsArchived = false,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            await _unitOfWork.Lookups.AddAsync(lookup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupTableDetailResponseDto>(lookup);
        }

        public async Task<LookupTableDetailResponseDto> UpdateLookupTableAsync(int lookupTableId, UpdateLookupTableRequestDto request, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            if (lookup.IsArchived)
            {
                throw new InvalidOperationException($"Lookup table '{lookupTableId}' is archived and cannot be modified.");
            }

            lookup.Name = request.Name;
            lookup.Description = request.Description;
            lookup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupTableDetailResponseDto>(lookup);
        }

        public async Task<LookupTableDetailResponseDto> ArchiveLookupTableAsync(int lookupTableId, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            lookup.IsArchived = true;
            lookup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupTableDetailResponseDto>(lookup);
        }

        public async Task<PagedResponseDto<LookupTableSummaryResponseDto>> GetLookupTablesAsync(PagedRequestDto request, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Lookups.GetQueryable().AsNoTracking().OrderBy(x => x.Name);
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponseDto<LookupTableSummaryResponseDto>
            {
                Items = _mapper.Map<IReadOnlyList<LookupTableSummaryResponseDto>>(items),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<LookupTableDetailResponseDto> GetLookupTableByIdAsync(int lookupTableId, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            lookup.Items = lookup.Items.OrderBy(x => x.Order).ToList();
            return _mapper.Map<LookupTableDetailResponseDto>(lookup);
        }

        public async Task<LookupItemResponseDto> CreateLookupItemAsync(int lookupTableId, CreateLookupItemRequestDto request, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            EnsureLookupEditable(lookup);
            EnsureUniqueLookupItemValue(lookup, request.Value);

            var utcNow = DateTime.UtcNow;
            var item = new LookupItem
            {
                Value = request.Value,
                Label = request.Label,
                Order = request.Order,
                IsActive = true,
                ParentValue = request.ParentValue,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            lookup.Items.Add(item);
            lookup.UpdatedAt = utcNow;
            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupItemResponseDto>(item);
        }

        public async Task<LookupItemResponseDto> UpdateLookupItemAsync(int lookupTableId, int itemId, UpdateLookupItemRequestDto request, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            EnsureLookupEditable(lookup);

            var item = lookup.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Lookup item '{itemId}' was not found in table '{lookupTableId}'.");

            item.Label = request.Label;
            item.Order = request.Order;
            item.ParentValue = request.ParentValue;
            item.UpdatedAt = DateTime.UtcNow;
            lookup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupItemResponseDto>(item);
        }

        public async Task<LookupItemResponseDto> SetLookupItemActiveStateAsync(int lookupTableId, int itemId, SetLookupItemActiveStateRequestDto request, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            EnsureLookupEditable(lookup);

            var item = lookup.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Lookup item '{itemId}' was not found in table '{lookupTableId}'.");

            item.IsActive = request.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
            lookup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LookupItemResponseDto>(item);
        }

        public async Task<IReadOnlyList<LookupItemResponseDto>> ReorderLookupItemsAsync(int lookupTableId, ReorderLookupItemsRequestDto request, CancellationToken cancellationToken = default)
        {
            var lookup = await LoadLookupTableWithItemsOrThrowAsync(lookupTableId, cancellationToken);
            EnsureLookupEditable(lookup);

            var existingIds = lookup.Items.Select(x => x.Id).ToHashSet();
            var requestIds = request.Items.Select(x => x.LookupItemId).ToHashSet();
            if (!existingIds.SetEquals(requestIds))
            {
                throw new InvalidOperationException("Reorder request must include all lookup items exactly once.");
            }

            foreach (var requestItem in request.Items)
            {
                var item = lookup.Items.First(x => x.Id == requestItem.LookupItemId);
                item.Order = requestItem.Order;
                item.UpdatedAt = DateTime.UtcNow;
            }

            lookup.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Lookups.Update(lookup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return lookup.Items
                .OrderBy(x => x.Order)
                .Select(x => _mapper.Map<LookupItemResponseDto>(x))
                .ToList();
        }

        public async Task<LookupOptionsResponseDto> GetLookupOptionsAsync(string lookupCode, CancellationToken cancellationToken = default)
        {
            var lookup = await _unitOfWork.Lookups.GetByCodeAsync(lookupCode, cancellationToken)
                ?? throw new KeyNotFoundException($"Lookup table with code '{lookupCode}' was not found.");

            if (lookup.IsArchived)
            {
                throw new KeyNotFoundException($"Lookup table with code '{lookupCode}' was not found.");
            }

            var activeItems = await _unitOfWork.Lookups.GetActiveItemsAsync(lookupCode, cancellationToken);
            return new LookupOptionsResponseDto
            {
                LookupCode = lookupCode,
                Items = _mapper.Map<IReadOnlyList<LookupOptionItemResponseDto>>(activeItems)
            };
        }

        private async Task<LookupTable> LoadLookupTableWithItemsOrThrowAsync(int lookupTableId, CancellationToken cancellationToken)
        {
            var lookup = await _unitOfWork.Lookups.GetQueryable()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == lookupTableId, cancellationToken);

            if (lookup is null)
            {
                throw new KeyNotFoundException($"Lookup table '{lookupTableId}' was not found.");
            }

            return lookup;
        }

        private static void EnsureLookupEditable(LookupTable lookup)
        {
            if (lookup.IsArchived)
            {
                throw new InvalidOperationException($"Lookup table '{lookup.Id}' is archived and cannot be modified.");
            }
        }

        private static void EnsureUniqueLookupItemValue(LookupTable lookup, string value)
        {
            if (lookup.Items.Any(x => x.Value == value))
            {
                throw new InvalidOperationException($"Lookup item value '{value}' already exists in table '{lookup.Id}'.");
            }
        }
    }
}
