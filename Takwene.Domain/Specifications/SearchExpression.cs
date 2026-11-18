using System.Linq.Expressions;

namespace Takwene.Domain.Specifications;

// One searchable column and the term to look for in it.
public sealed record SearchExpression<T>(Expression<Func<T, string>> Selector, string Term);
