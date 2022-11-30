using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OperatorToken { get; }
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
