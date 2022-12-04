using System;

namespace Balu.Binding;

sealed class BoundAssignmentExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override Type Type => Expression.Type;
    public string Name { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(string name, BoundExpression expression) => (Name, Expression) = (name, expression);

    internal override BoundExpression Accept(BoundExpressionVisitor visitor)
    {
        var expression = visitor.Visit(Expression);
        return expression == Expression ? this : new BoundAssignmentExpression(Name, expression);
    }
}