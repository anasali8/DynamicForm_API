using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Infrastructure.Repositories
{
    public class LookupRepository : GenericRepository<LookupTable>, ILookupRepository
    {
        private readonly AppDbContext _context;

        public LookupRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LookupTable?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.LookupTables
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<IEnumerable<LookupItem>> GetActiveItemsAsync(string lookupCode, CancellationToken cancellationToken = default)
        {
            return await _context.LookupItems
                .AsNoTracking()
                .Where(x => x.LookupTable.Code == lookupCode && !x.LookupTable.IsArchived && x.IsActive)
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);
        }
    }
}
