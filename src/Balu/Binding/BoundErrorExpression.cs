using System;
using Balu.Symbols;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundErrorExpression : BoundExpression
{
    public override TypeSymbol Type => TypeSymbol.Error;
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

    internal BoundErrorExpression(SyntaxNode syntax)
        : base(syntax)
    {
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => "?";
}
