﻿using System;

namespace Balu.Syntax;

public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

    internal BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }
}
