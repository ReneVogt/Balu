using System;
using System.Collections.Generic;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override TypeSymbol Type => Variable.Type;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public VariableSymbol Variable { get; }

    public BoundVariableExpression(VariableSymbol variable) => Variable = variable;

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => Variable.Name;
}