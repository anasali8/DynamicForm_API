using DynamicForm_API.Core.Interfaces;
using DynamicForm_API.Infrastructure.Data;
using DynamicForm_API.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DynamicForm_API.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Forms = new FormRepository(_context);
            FormVersions = new FormVersionRepository(_context);
            Lookups = new LookupRepository(_context);
            Submissions = new FormSubmissionRepository(_context);
        }

        public IFormRepository Forms { get; }
        public IFormVersionRepository FormVersions { get; }
        public ILookupRepository Lookups { get; }
        public IFormSubmissionRepository Submissions { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                return;
            }

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
            {
                return;
            }

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
