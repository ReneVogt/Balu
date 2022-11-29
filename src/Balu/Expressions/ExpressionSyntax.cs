﻿namespace Balu.Expressions;

/// <summary>
/// The abstract base class for expresssion <see cref="SyntaxNode">syntax nodes</see>.
/// </summary>
public abstract class ExpressionSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="NumberExpressionSyntax"/> from the given <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="numberToken">The <see cref="SyntaxToken"/> creating this expression.</param>
    /// <returns>The parsed <see cref="NumberExpressionSyntax"/>.</returns>
    public static NumberExpressionSyntax Number(SyntaxToken numberToken) => new(numberToken);
    /// <summary>
    /// Creates a new <see cref="BinaryExpressionSyntax"/> from the given <paramref name="left"/> and <paramref name="right"/> <see cref="ExpressionSyntax"/> expressions
    /// and the given <paramref name="operatorToken"/> 
    /// </summary>
    /// <param name="left">The <see cref="ExpressionSyntax"/> representing the left operand.</param>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="right">The <see cref="ExpressionSyntax"/> representing the right operand.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    public static BinaryExpressionSyntax Binary(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right) => new(left, operatorToken, right);
    /// <summary>
    /// Creates a new <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="openParenthesisToken">The <see cref="SyntaxToken"/> representing the opening parenthesis.</param>
    /// <param name="expresssion">The <see cref="ExpressionSyntax"/> representing the inner expression.</param>
    /// <param name="closedParenthesisToken">The <see cref="SyntaxToken"/> representing the closing parenthesis.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    public static ParenthesizedExpressionSyntax Parenthesized(SyntaxToken openParenthesisToken, ExpressionSyntax expresssion, SyntaxToken closedParenthesisToken) => new(openParenthesisToken, expresssion, closedParenthesisToken);
}
