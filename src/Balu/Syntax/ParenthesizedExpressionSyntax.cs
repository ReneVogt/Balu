using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a parenthesized expression.
/// </summary>
public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The token for the opening parenthesis.
    /// </summary>
    public SyntaxToken OpenParenthesisToken { get; }
    /// <summary>
    /// The expression inside the parenthesis.
    /// </summary>
    public ExpressionSyntax Expression { get; }
    /// <summary>
    /// The token for the closing parenthesis.
    /// </summary>
    public SyntaxToken ClosedParenthesisToken { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return ClosedParenthesisToken;
        }
    }

    internal ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closedParenthesisToken) =>
        (OpenParenthesisToken, Expression, ClosedParenthesisToken) = (openParenthesisToken, expression, closedParenthesisToken);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken open = (SyntaxToken)visitor.Visit(OpenParenthesisToken);
        ExpressionSyntax expr = (ExpressionSyntax)visitor.Visit(Expression);
        SyntaxToken close = (SyntaxToken)visitor.Visit(ClosedParenthesisToken);
        return open != OpenParenthesisToken || expr != Expression || close != ClosedParenthesisToken ? Parenthesized(open, expr,close) : this;
    }

}
