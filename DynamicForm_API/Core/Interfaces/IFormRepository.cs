using DynamicForm_API.Core.Models.Entities;
using System.Threading;

namespace DynamicForm_API.Core.Interfaces
{
    public interface IFormRepository : IGenericRepository<Form>
    {
        Task<Form?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<IEnumerable<Form>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
