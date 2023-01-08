using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => $"nop";
}
