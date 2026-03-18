using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Core.Models.Entities;
using DynamicForm_API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Infrastructure.Repositories
{
    public class FormSubmissionRepository : GenericRepository<FormSubmission>, IFormSubmissionRepository
    {
        private readonly AppDbContext _context;

        public FormSubmissionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormSubmission>> GetByFormIdAsync(int formId, int pageNumber, int pageSize, string? userId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.FormSubmissions
                .AsNoTracking()
                .Where(x => x.FormId == formId);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            return await query
                .OrderByDescending(x => x.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<FormSubmission?> GetDetailsAsync(int submissionId, CancellationToken cancellationToken = default)
        {
            return await _context.FormSubmissions
                .AsNoTracking()
                .Include(x => x.Form)
                .Include(x => x.FormVersion)
                .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);
        }

        public async Task<FormSubmission?> GetDetailsByIdWithIncludesAsync(int submissionId, CancellationToken cancellationToken = default)
        {
            return await _context.FormSubmissions
                .AsNoTracking()
                .Include(x => x.Form)
                .Include(x => x.FormVersion)
                    .ThenInclude(v => v.Fields)
                .FirstOrDefaultAsync(x => x.Id == submissionId, cancellationToken);
        }
    }
}
