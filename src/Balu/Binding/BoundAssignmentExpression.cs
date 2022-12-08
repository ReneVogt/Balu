using System;

namespace Balu.Binding;

sealed class BoundAssignmentExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    public override Type Type => Expression.Type;
    public VariableSymbol Symbol { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(VariableSymbol symbol, BoundExpression expression) => (Symbol, Expression) = (symbol, expression);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new (Symbol, expression);
    }
}