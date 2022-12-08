using System;

namespace Balu.Binding;

sealed class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override Type Type => Symbol.Type;
    public VariableSymbol Symbol { get; }

    public BoundVariableExpression(VariableSymbol symbol) => Symbol = symbol;

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;
}