using System;

namespace Balu.Binding;

sealed class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override Type Type { get; }
    public string Name { get; }

    public BoundVariableExpression(string name, Type type) => (Name, Type) = (name, type);

    internal override BoundExpression Accept(BoundExpressionVisitor visitor) => this;
}