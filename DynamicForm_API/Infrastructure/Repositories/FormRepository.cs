using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Infrastructure.Repositories
{
    public class FormRepository : GenericRepository<Form>, IFormRepository
    {
        private readonly AppDbContext _context;

        public FormRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Form?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Forms
                .Include(x => x.Versions)
                .ThenInclude(v => v.Fields)
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Forms.AnyAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<IEnumerable<Form>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Forms
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
