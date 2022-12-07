using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a unary expression.
/// </summary>
public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The token for the unary operator.
    /// </summary>
    public SyntaxToken OperatorToken { get; }
    /// <summary>
    /// The expression the unary operator is applied to.
    /// </summary>
    public ExpressionSyntax Expression { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return OperatorToken;
            yield return Expression;
        }
    }

    internal UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax expression) =>
        (OperatorToken, Expression) = (operatorToken, expression);
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken operatorToken = (SyntaxToken)visitor.Visit(OperatorToken);
        ExpressionSyntax expression = (ExpressionSyntax)visitor.Visit(Expression);
        return operatorToken != OperatorToken || expression != Expression ? Unary(operatorToken, expression) : this;
    }
}
