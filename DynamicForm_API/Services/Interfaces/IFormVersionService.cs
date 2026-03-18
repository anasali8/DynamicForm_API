using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.API.DTOs.Versions;

namespace DynamicForm_API.Services.Interfaces
{
    public interface IFormVersionService
    {
        Task<FormVersionDetailResponseDto> CreateNextDraftVersionAsync(int formId, CreateNextDraftVersionRequestDto request, CancellationToken cancellationToken = default);
        Task<FormVersionDetailResponseDto> UpdateDraftVersionAsync(int formId, int versionId, UpdateDraftVersionRequestDto request, CancellationToken cancellationToken = default);
        Task<FormVersionDetailResponseDto> PublishDraftVersionAsync(int formId, int versionId, CancellationToken cancellationToken = default);
        Task<FormVersionDetailResponseDto> ArchiveVersionAsync(int formId, int versionId, CancellationToken cancellationToken = default);
        Task<PagedResponseDto<FormVersionHistoryItemResponseDto>> GetVersionsByFormIdAsync(int formId, PagedRequestDto request, CancellationToken cancellationToken = default);
        Task<FormVersionDetailResponseDto> GetVersionDetailsAsync(int formId, int versionId, CancellationToken cancellationToken = default);

        Task<FormFieldResponseDto> AddDraftFieldAsync(int formId, int versionId, AddDraftFieldRequestDto request, CancellationToken cancellationToken = default);
        Task<FormFieldResponseDto> UpdateDraftFieldAsync(int formId, int versionId, int fieldId, UpdateDraftFieldRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> RemoveDraftFieldAsync(int formId, int versionId, int fieldId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FormFieldResponseDto>> ReorderDraftFieldsAsync(int formId, int versionId, ReorderDraftFieldsRequestDto request, CancellationToken cancellationToken = default);
    }
}
