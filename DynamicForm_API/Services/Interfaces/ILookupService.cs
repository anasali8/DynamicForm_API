using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Lookups;

namespace DynamicForm_API.Services.Interfaces
{
    public interface ILookupService
    {
        Task<LookupTableDetailResponseDto> CreateLookupTableAsync(CreateLookupTableRequestDto request, CancellationToken cancellationToken = default);
        Task<LookupTableDetailResponseDto> UpdateLookupTableAsync(int lookupTableId, UpdateLookupTableRequestDto request, CancellationToken cancellationToken = default);
        Task<LookupTableDetailResponseDto> ArchiveLookupTableAsync(int lookupTableId, CancellationToken cancellationToken = default);
        Task<PagedResponseDto<LookupTableSummaryResponseDto>> GetLookupTablesAsync(PagedRequestDto request, CancellationToken cancellationToken = default);
        Task<LookupTableDetailResponseDto> GetLookupTableByIdAsync(int lookupTableId, CancellationToken cancellationToken = default);

        Task<LookupItemResponseDto> CreateLookupItemAsync(int lookupTableId, CreateLookupItemRequestDto request, CancellationToken cancellationToken = default);
        Task<LookupItemResponseDto> UpdateLookupItemAsync(int lookupTableId, int itemId, UpdateLookupItemRequestDto request, CancellationToken cancellationToken = default);
        Task<LookupItemResponseDto> SetLookupItemActiveStateAsync(int lookupTableId, int itemId, SetLookupItemActiveStateRequestDto request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemResponseDto>> ReorderLookupItemsAsync(int lookupTableId, ReorderLookupItemsRequestDto request, CancellationToken cancellationToken = default);

        Task<LookupOptionsResponseDto> GetLookupOptionsAsync(string lookupCode, CancellationToken cancellationToken = default);
    }
}
