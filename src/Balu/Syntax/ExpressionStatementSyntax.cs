using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents an expression statement.
/// </summary>
public sealed class ExpressionStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get { yield return Expression; }
    }

    /// <summary>
    /// The expression in this statement.
    /// </summary>
    public ExpressionSyntax Expression { get; }

    internal ExpressionStatementSyntax(ExpressionSyntax expression) => Expression = expression;
    
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        throw new System.NotImplementedException();
    }
}
