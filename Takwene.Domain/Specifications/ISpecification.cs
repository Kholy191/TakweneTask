using System.Linq.Expressions;

namespace Takwene.Domain.Specifications;

// Describes a query (filter, includes, sorting, paging) without depending on EF Core.
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }

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

// Specification that projects T into TResult (e.g. a DTO) in SQL.
public interface ISpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>> Selector { get; }
}
