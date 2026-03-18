using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Forms;

namespace DynamicForm_API.Services.Interfaces
{
    public interface IFormService
    {
        Task<FormDetailResponseDto> CreateFormAsync(CreateFormDraftRequestDto request, CancellationToken cancellationToken = default);
        Task<FormDetailResponseDto> UpdateFormMetadataAsync(int formId, UpdateFormMetadataRequestDto request, CancellationToken cancellationToken = default);
        Task<FormDetailResponseDto> ArchiveFormAsync(int formId, CancellationToken cancellationToken = default);
        Task<PagedResponseDto<FormSummaryResponseDto>> GetFormsAsync(PagedRequestDto request, CancellationToken cancellationToken = default);
        Task<FormDetailResponseDto> GetFormByIdAsync(int formId, CancellationToken cancellationToken = default);
        Task<FormWithCurrentPublishedVersionResponseDto> GetCurrentPublishedFormAsync(string formCode, CancellationToken cancellationToken = default);
    }
}