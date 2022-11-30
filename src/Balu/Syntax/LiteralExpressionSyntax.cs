﻿using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The <see cref="SyntaxToken"/> of this expression.
    /// </summary>
    public SyntaxToken LiteralToken { get; }
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return LiteralToken;
        }
    }

    internal LiteralExpressionSyntax(SyntaxToken literalToken) => LiteralToken = literalToken;
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken literal = (SyntaxToken)visitor.Visit(LiteralToken);
        return literal == LiteralToken ? this : Literal(literal);
    }
}
