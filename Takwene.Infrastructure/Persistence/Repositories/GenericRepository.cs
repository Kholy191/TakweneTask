using Microsoft.EntityFrameworkCore;
using Takwene.Domain.Common;
using Takwene.Domain.Interfaces;

namespace Takwene.Infrastructure.Persistence.Repositories;

internal sealed class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync(new object[] { id }, cancellationToken);

    // Read-only listing, so no tracking.
    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator.GetQuery(_set, spec).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator.GetQuery(_set, spec).ToListAsync(cancellationToken);

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator.GetQuery(_set, spec).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default) =>
        await SpecificationEvaluator.GetQuery(_set, spec).ToListAsync(cancellationToken);

    public Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) =>
        SpecificationEvaluator.GetQuery(_set, spec, criteriaOnly: true).CountAsync(cancellationToken);

    public Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default) =>
        SpecificationEvaluator.GetQuery(_set, spec, criteriaOnly: true).AnyAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Delete(T entity) => _set.Remove(entity);
}
