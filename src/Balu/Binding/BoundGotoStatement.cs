using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundGotoStatement : BoundStatement
{
    public BoundLabel Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public BoundGotoStatement(BoundLabel label) => Label = label;

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => $"goto {Label}";
}
