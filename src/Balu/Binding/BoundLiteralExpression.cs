using System;

namespace Balu.Binding;

sealed class BoundLiteralExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override Type Type => Value.GetType();

    public object Value { get; }

    public BoundLiteralExpression(object value) => Value = value;

    internal override BoundExpression Accept(BoundExpressionVisitor visitor) => this;
}