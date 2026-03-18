namespace DynamicForm_API.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IFormRepository Forms { get; }
        IFormVersionRepository FormVersions { get; }
        ILookupRepository Lookups { get; }
        IFormSubmissionRepository Submissions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
