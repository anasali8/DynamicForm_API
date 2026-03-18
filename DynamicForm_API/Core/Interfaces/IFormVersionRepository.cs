using DynamicForm_API.Core.Models.Entities;

namespace DynamicForm_API.Core.Interfaces
{
    public interface IFormVersionRepository : IGenericRepository<FormVersion>
    {
        Task<FormVersion?> GetCurrentPublishedVersionAsync(int formId, CancellationToken cancellationToken = default);

        Task<FormVersion?> GetByVersionNumberAsync(int formId, int versionNumber, CancellationToken cancellationToken = default);

        Task<IEnumerable<FormVersion>> GetVersionsByFormIdAsync(int formId, CancellationToken cancellationToken = default);

        Task<int> GetNextVersionNumberAsync(int formId, CancellationToken cancellationToken = default);
    }
}
