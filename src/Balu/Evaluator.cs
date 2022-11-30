using System;
using System.Data;
using Balu.Expressions;

namespace Balu;

/// <summary>
/// An evaluator for an <see cref="ExpressionSyntax"/>.
/// </summary>
public sealed class Evaluator
{
    readonly ExpressionSyntax root;

    /// <summary>
    /// Creates a new <see cref="Evaluator"/> for the given <see cref="ExpressionSyntax"/>.
    /// </summary>
    /// <param name="root">The root <see cref="ExpressionSyntax"/> to evaluate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    public Evaluator(ExpressionSyntax root) => this.root = root ?? throw new ArgumentNullException(nameof(root));

    public int Evaluate() => Evaluate(root);

    static int Evaluate(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax { LiteralToken.Value: int number } => number,
        UnaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.PlusToken, Expression: var operand } => Evaluate(operand),
        UnaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.MinusToken, Expression: var operand } => -Evaluate(operand),
        BinaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.PlusToken, Left: var left, Right: var right } => Evaluate(left) + Evaluate(right),
        BinaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.MinusToken, Left: var left, Right: var right } => Evaluate(left) - Evaluate(right),
        BinaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.StarToken, Left: var left, Right: var right } => Evaluate(left) * Evaluate(right),
        BinaryExpressionSyntax { OperatorToken.Kind: SyntaxKind.SlashToken, Left: var left, Right: var right } => Evaluate(left) / Evaluate(right),
        ParenthesizedExpressionSyntax { Expression: var innerExpression} => Evaluate(innerExpression),
        _ => throw new InvalidOperationException($"Expressions {expression} cannot be evaluated.")
    };
}
