using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Submissions;

namespace DynamicForm_API.Services.Interfaces
{
    public interface ISubmissionService
    {
        Task<FormSubmissionCreatedResponseDto> SubmitFormAsync(string formCode, SubmitFormVersionRequestDto request, CancellationToken cancellationToken = default);
        Task<PagedResponseDto<FormSubmissionListItemResponseDto>> GetFormSubmissionsAsync(string formCode, PagedRequestDto request, string? userId = null, CancellationToken cancellationToken = default);
        Task<FormSubmissionDetailResponseDto> GetSubmissionDetailsAsync(int formId, int submissionId, CancellationToken cancellationToken = default);
        Task<FormSubmissionDetailResponseDto?> GetSubmissionDetailsByIdAsync(int submissionId, CancellationToken cancellationToken = default);
    }
}
