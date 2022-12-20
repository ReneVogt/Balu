using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'return' statement.
/// </summary>
public sealed class ReturnStatementSyntax : StatementSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ReturnKeyword;
            if (Expression is not null) yield return Expression;
        }
    }

    /// <summary>
    /// The 'return' keyword token.
    /// </summary>
    public SyntaxToken ReturnKeyword { get; }
    /// <summary>
    /// The optional return expression.
    /// </summary>
    public ExpressionSyntax? Expression { get; }

    internal ReturnStatementSyntax(SyntaxToken returnKeyword, ExpressionSyntax? expression) => (ReturnKeyword, Expression) = (returnKeyword, expression);

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var returnKeyword = (SyntaxToken)visitor.Visit(ReturnKeyword);
        var expression = Expression is null ? null : (ExpressionSyntax)visitor.Visit(Expression);
        return returnKeyword == ReturnKeyword && expression == Expression ? this : ReturnStatement(returnKeyword, expression);
    }
}
