using Takwene.Application.Common;
using Takwene.Domain.Common;
using Takwene.Domain.Interfaces;

namespace Takwene.Application.Interfaces;

// Hands out repositories that share one DbContext and commits their changes together.
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    // Persists all pending changes; returns the number of affected rows.
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);

    // Runs the action in one DB transaction: commits on success, rolls back on failure or exception.
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
        where TResult : Result;
}
