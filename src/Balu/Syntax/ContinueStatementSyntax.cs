using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'continue' statement.
/// </summary>
public sealed class ContinueStatementSyntax : StatementSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ContinueKeyword;
        }
    }

    public SyntaxToken ContinueKeyword { get; }

    internal ContinueStatementSyntax(SyntaxToken continueKeyword) => ContinueKeyword = continueKeyword;

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var continueKeyword = (SyntaxToken)visitor.Visit(ContinueKeyword);
        return continueKeyword == ContinueKeyword ? this : ContinueStatement(continueKeyword);
    }
}
