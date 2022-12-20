using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundExpressionStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Expression;
        }
    }

    public BoundExpression Expression { get; }

    public BoundExpressionStatement(BoundExpression expression)
    {
        Expression = expression;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new (expression);
    }

    public override string ToString() => Expression.ToString();
}