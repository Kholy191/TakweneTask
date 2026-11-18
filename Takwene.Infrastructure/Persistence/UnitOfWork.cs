using Microsoft.EntityFrameworkCore;
using Takwene.Application.Common;
using Takwene.Application.Interfaces;
using Takwene.Domain.Common;
using Takwene.Domain.Interfaces;
using Takwene.Infrastructure.Persistence.Repositories;

namespace Takwene.Infrastructure.Persistence;

internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    // One repository instance per entity type for the lifetime of the request.
    private readonly Dictionary<Type, object> _repositories = [];

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        if (!_repositories.TryGetValue(typeof(T), out var repository))
        {
            repository = new GenericRepository<T>(context);
            _repositories[typeof(T)] = repository;
        }

        return (IGenericRepository<T>)repository;
    }

    public Task<int> CompleteAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
        where TResult : Result
    {
        // Wrapped in the execution strategy so it stays valid if connection retries are enabled later.
        var strategy = context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            // UserManager and repositories share this DbContext, so both join the transaction.
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var result = await action();

            if (result.IsSuccess)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);

            return result;
        });
    }
}
