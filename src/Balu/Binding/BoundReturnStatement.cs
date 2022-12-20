using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundReturnStatement : BoundStatement
{
    public BoundExpression? Expression { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            if (Expression is not null) yield return Expression;

        }
    }

    public BoundReturnStatement(BoundExpression? expression) => Expression = expression;

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = Expression is null ? null : (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new (expression);
    }

    public override string ToString() => $"return {Expression}";
}
