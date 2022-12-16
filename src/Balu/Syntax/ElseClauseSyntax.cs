using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents an 'else' clause and its statement.
/// </summary>
public sealed class ElseClauseSyntax : SyntaxNode
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.ElseClause;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ElseKeyword;
            yield return Statement;
        }
    }

    /// <summary>
    /// The 'else' keyword.
    /// </summary>
    public SyntaxToken ElseKeyword { get; }
    /// <summary>
    /// The statement to execute in 'else' case.
    /// </summary>
    public StatementSyntax Statement { get; }

    internal ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax statement) => (ElseKeyword, Statement) = (elseKeyword, statement);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var keyword = (SyntaxToken)visitor.Visit(ElseKeyword);
        var statement = (StatementSyntax)visitor.Visit(Statement);
        return keyword == ElseKeyword && statement == Statement ? this : Else(keyword, statement);
    }
}
