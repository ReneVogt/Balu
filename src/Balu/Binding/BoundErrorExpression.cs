using System;
using Balu.Symbols;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundErrorExpression : BoundExpression
{
    public override TypeSymbol Type => TypeSymbol.Error;
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

    public override IEnumerable<BoundNode> Children => Array.Empty<BoundNode>();

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;

    public override string ToString() => "?";
}
