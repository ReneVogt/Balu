using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a binary expression.
/// </summary>
public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The left operand.
    /// </summary>
    public ExpressionSyntax Left { get; }
    /// <summary>
    /// The binary operator.
    /// </summary>
    public SyntaxToken OperatorToken { get; }
    /// <summary>
    /// The right operand.
    /// </summary>
    public ExpressionSyntax Right { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    internal BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right) =>
        (Left, OperatorToken, Right) = (left, operatorToken, right);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        ExpressionSyntax left = (ExpressionSyntax)visitor.Visit(Left);
        SyntaxToken operatorToken = (SyntaxToken)visitor.Visit(OperatorToken);
        ExpressionSyntax right = (ExpressionSyntax)visitor.Visit(Right);
        return left != Left || operatorToken != OperatorToken || right != Right ? Binary(left, operatorToken, right) : this;
    }
}
