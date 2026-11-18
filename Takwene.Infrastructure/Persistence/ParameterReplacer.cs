using System.Linq.Expressions;

namespace Takwene.Infrastructure.Persistence;

// Swaps one lambda parameter for another expression, so separate lambdas can be combined into one.
internal sealed class ParameterReplacer(ParameterExpression source, Expression target) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) =>
        node == source ? target : base.VisitParameter(node);
}
