using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents an assignment expression.
/// </summary>
public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The identifier token that identifies the target of the assignment.
    /// </summary>
    public SyntaxToken IdentifierrToken { get; }
    /// <summary>
    /// The equals token of this assignment.
    /// </summary>
    public SyntaxToken EqualsToken { get; }
    /// <summary>
    /// The expression of this assignment.
    /// </summary>
    public ExpressionSyntax Expression { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return IdentifierrToken;
            yield return EqualsToken;
            yield return Expression;
        }
    }

    internal AssignmentExpressionSyntax(SyntaxToken identifierrToken, SyntaxToken equalsToken, ExpressionSyntax expression) =>
        (IdentifierrToken, EqualsToken, Expression) = (identifierrToken, equalsToken, expression);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken identifierToken = (SyntaxToken)visitor.Visit(IdentifierrToken);
        SyntaxToken equalsToken = (SyntaxToken)visitor.Visit(EqualsToken);
        ExpressionSyntax expression = (ExpressionSyntax)visitor.Visit(Expression);
        return identifierToken != IdentifierrToken || equalsToken != EqualsToken || expression != Expression ? Assignment(identifierToken, equalsToken, expression) : this;
    }
}
