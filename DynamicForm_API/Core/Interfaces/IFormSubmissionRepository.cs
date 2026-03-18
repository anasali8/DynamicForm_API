using DynamicForm_API.Core.Models.Entities;
using System.Threading;

namespace DynamicForm_API.Core.Interfaces
{
    public interface IFormSubmissionRepository : IGenericRepository<FormSubmission>
    {
        Task<IEnumerable<FormSubmission>> GetByFormIdAsync(int formId, int pageNumber, int pageSize, string? userId = null, CancellationToken cancellationToken = default);

        Task<FormSubmission?> GetDetailsAsync(int submissionId, CancellationToken cancellationToken = default);

        Task<FormSubmission?> GetDetailsByIdWithIncludesAsync(int submissionId, CancellationToken cancellationToken = default);
    }
}
