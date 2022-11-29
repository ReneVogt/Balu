using System.Collections.Generic;

namespace Balu.Expressions;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
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
}
