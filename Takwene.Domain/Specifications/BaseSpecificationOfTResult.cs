using System.Linq.Expressions;
using Takwene.Domain.Interfaces;

namespace Takwene.Domain.Specifications;

// Base class for projection specifications. Projected results are never tracked by EF Core.
public abstract class BaseSpecification<T, TResult> : BaseSpecification<T>, ISpecification<T, TResult>
{
    protected BaseSpecification() { }

    protected BaseSpecification(Expression<Func<T, bool>> criteria) : base(criteria) { }

    public Expression<Func<T, TResult>> Selector { get; private set; } = null!;

    protected void ApplySelector(Expression<Func<T, TResult>> selector) => Selector = selector;
}
