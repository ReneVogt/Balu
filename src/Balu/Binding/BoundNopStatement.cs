using System;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    internal BoundNopStatement(SyntaxNode syntax) : base(syntax){}

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => "nop";
}
