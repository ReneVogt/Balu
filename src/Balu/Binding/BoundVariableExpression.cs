using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override Type Type => Symbol.Type;
    public override IEnumerable<BoundNode> Children => Array.Empty<BoundNode>();

    public VariableSymbol Symbol { get; }

    public BoundVariableExpression(VariableSymbol symbol) => Symbol = symbol;

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;
}