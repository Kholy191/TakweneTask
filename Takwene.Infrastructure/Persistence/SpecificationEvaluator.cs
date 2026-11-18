using Microsoft.EntityFrameworkCore;
using Takwene.Domain.Common;
using Takwene.Domain.Specifications;

namespace Takwene.Infrastructure.Persistence;

// Turns a specification into an EF Core query.
internal static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> spec, bool criteriaOnly = false)
        where T : BaseEntity
    {
        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        // Count/Any must ignore paging, sorting and includes.
        if (criteriaOnly)
            return query;

        if (!spec.IsTracking)
            query = query.AsNoTracking();

        if (spec.IsSplitQuery)
            query = query.AsSplitQuery();

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);
        else if (spec.IsPagingEnabled)
            query = query.OrderBy(e => e.Id); // Paging needs a stable order.

        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);

        return query;
    }

    public static IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query, ISpecification<T, TResult> spec)
        where T : BaseEntity
    {
        var selector = spec.Selector ?? throw new InvalidOperationException($"{spec.GetType().Name} has no selector.");
        return GetQuery(query, (ISpecification<T>)spec).Select(selector);
    }
}
