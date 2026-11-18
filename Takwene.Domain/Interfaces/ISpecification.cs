using System.Linq.Expressions;
using Takwene.Domain.Specifications;

namespace Takwene.Domain.Interfaces;

// Describes a query (filter, search, includes, sorting, paging) without depending on EF Core.
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

    // Case-insensitive "contains" searches, OR-ed together; translated to ILIKE by Infrastructure.
    List<SearchExpression<T>> SearchExpressions { get; }

    List<Expression<Func<T, object>>> Includes { get; }

    // String includes support nested paths, e.g. "Tracks.Distributions".
    List<string> IncludeStrings { get; }

    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }

    int Skip { get; }
    int Take { get; }
    bool IsPagingEnabled { get; }

    // Tracking is on by default; read-only specs should turn it off.
    bool IsTracking { get; }

    // Splits multiple collection includes into separate SQL queries.
    bool IsSplitQuery { get; }
}
