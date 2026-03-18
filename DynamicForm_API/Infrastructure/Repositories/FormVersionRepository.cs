using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Infrastructure.Data;
using DynamicForm_API.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Infrastructure.Repositories
{
    public class FormVersionRepository : GenericRepository<FormVersion>, IFormVersionRepository
    {
        private readonly AppDbContext _context;

        public FormVersionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FormVersion?> GetCurrentPublishedVersionAsync(int formId, CancellationToken cancellationToken = default)
        {
            return await _context.FormVersions
                .Include(x => x.Fields.OrderBy(f => f.Order))
                .FirstOrDefaultAsync(x => x.FormId == formId && x.Status == FormVersionStatus.Published && x.IsCurrent, cancellationToken);
        }

        public async Task<FormVersion?> GetByVersionNumberAsync(int formId, int versionNumber, CancellationToken cancellationToken = default)
        {
            return await _context.FormVersions
                .Include(x => x.Fields.OrderBy(f => f.Order))
                .FirstOrDefaultAsync(x => x.FormId == formId && x.VersionNumber == versionNumber, cancellationToken);
        }

        public async Task<IEnumerable<FormVersion>> GetVersionsByFormIdAsync(int formId, CancellationToken cancellationToken = default)
        {
            return await _context.FormVersions
                .AsNoTracking()
                .Where(x => x.FormId == formId)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetNextVersionNumberAsync(int formId, CancellationToken cancellationToken = default)
        {
            var currentMax = await _context.FormVersions
                .Where(x => x.FormId == formId)
                .Select(x => (int?)x.VersionNumber)
                .MaxAsync(cancellationToken);

            return (currentMax ?? 0) + 1;
        }
    }
}
