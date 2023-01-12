﻿using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    internal BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }
}
