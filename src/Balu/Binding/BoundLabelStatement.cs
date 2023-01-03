using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabel Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public BoundLabelStatement(BoundLabel label) => Label = label;

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => Label.ToString();
}
