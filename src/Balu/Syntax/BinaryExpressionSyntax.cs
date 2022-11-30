using System.Collections.Generic;

namespace Balu.Syntax;

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

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        ExpressionSyntax left = (ExpressionSyntax)visitor.Visit(Left);
        SyntaxToken operatorToken = (SyntaxToken)visitor.Visit(OperatorToken);
        ExpressionSyntax right = (ExpressionSyntax)visitor.Visit(Right);
        return left != Left || operatorToken != OperatorToken || right != Right ? Binary(left, operatorToken, right) : this;
    }
}
