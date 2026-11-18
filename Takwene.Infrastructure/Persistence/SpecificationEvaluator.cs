using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Takwene.Domain.Common;
using Takwene.Domain.Interfaces;
using Takwene.Domain.Specifications;

namespace Takwene.Infrastructure.Persistence;

// Turns a specification into an EF Core query.
internal static class SpecificationEvaluator
{
    private const string LikeEscapeCharacter = "\\";

    public static IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> spec, bool criteriaOnly = false)
        where T : BaseEntity
    {
        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        query = ApplySearch(query, spec.SearchExpressions);

        // Count/Any must ignore paging, sorting and includes (but keep filters and search).
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

    // Builds: col1 ILIKE @p1 ESCAPE '\' OR col2 ILIKE @p2 ESCAPE '\' ... so pg_trgm GIN indexes can be used.
    private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, IReadOnlyCollection<SearchExpression<T>> searches)
    {
        if (searches.Count == 0)
            return query;

        var entity = Expression.Parameter(typeof(T), "e");
        Expression? body = null;

        foreach (var search in searches)
        {
            // Captured in a closure so EF sends it as a parameter, not inlined SQL.
            var pattern = $"%{EscapeLikePattern(search.Term)}%";
            Expression<Func<string, bool>> ilike = value => EF.Functions.ILike(value, pattern, LikeEscapeCharacter);

            var column = new ParameterReplacer(search.Selector.Parameters[0], entity).Visit(search.Selector.Body);
            var match = new ParameterReplacer(ilike.Parameters[0], column).Visit(ilike.Body);

            body = body is null ? match : Expression.OrElse(body, match);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(body!, entity));
    }

    // User input like "50%" must match literally, not as a wildcard.
    private static string EscapeLikePattern(string term) => term
        .Replace(LikeEscapeCharacter, LikeEscapeCharacter + LikeEscapeCharacter)
        .Replace("%", LikeEscapeCharacter + "%")
        .Replace("_", LikeEscapeCharacter + "_");
}
