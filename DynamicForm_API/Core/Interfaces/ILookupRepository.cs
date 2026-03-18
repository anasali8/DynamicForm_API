using DynamicForm_API.Core.Models.Entities;

namespace DynamicForm_API.Core.Interfaces
{
    public interface ILookupRepository : IGenericRepository<LookupTable>
    {
        Task<LookupTable?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        Task<IEnumerable<LookupItem>> GetActiveItemsAsync(string lookupCode, CancellationToken cancellationToken = default);
    }
}
