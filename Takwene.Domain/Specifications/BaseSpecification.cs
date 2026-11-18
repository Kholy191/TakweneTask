using System.Linq.Expressions;
using Takwene.Domain.Interfaces;

namespace Takwene.Domain.Specifications;

// Base class for concrete specifications; derive and call the protected helpers.
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification() { }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; }
    public List<SearchExpression<T>> SearchExpressions { get; } = [];
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool IsTracking { get; private set; } = true;
    public bool IsSplitQuery { get; private set; }

    protected void AddSearch(Expression<Func<T, string>> selector, string term) =>
        SearchExpressions.Add(new SearchExpression<T>(selector, term));

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);

    protected void AddInclude(string includePath) => IncludeStrings.Add(includePath);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;

    // pageNumber starts at 1.
    protected void ApplyPaging(int pageNumber, int pageSize)
    {
        Skip = (pageNumber - 1) * pageSize;
        Take = pageSize;
        IsPagingEnabled = true;
    }

    protected void AsNoTracking() => IsTracking = false;

    protected void AsSplitQuery() => IsSplitQuery = true;
}
