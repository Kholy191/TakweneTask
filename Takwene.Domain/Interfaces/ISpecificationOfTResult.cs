using System.Linq.Expressions;

namespace Takwene.Domain.Interfaces;

// Specification that projects T into TResult (e.g. a DTO) in SQL.
public interface ISpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>> Selector { get; }
}
