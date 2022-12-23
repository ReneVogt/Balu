﻿using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken LiteralToken { get; }
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return LiteralToken;
        }
    }

    public object? Value { get; }

    public LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken) : this(syntaxTree, literalToken ?? throw new ArgumentNullException(nameof(literalToken)), literalToken.Value){}
    public LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken, object? value) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        LiteralToken = literalToken ?? throw new ArgumentNullException(nameof(literalToken));
        Value = value;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken literal = (SyntaxToken)visitor.Visit(LiteralToken);
        return literal == LiteralToken ? this : new(SyntaxTree, literal, literal.Value);
    }
}
