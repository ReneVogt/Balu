using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'break' statement.
/// </summary>
public sealed class BreakStatementSyntax : StatementSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return BreakKeyword;
        }
    }

    public SyntaxToken BreakKeyword { get; }

    internal BreakStatementSyntax(SyntaxToken breakKeyword) => BreakKeyword = breakKeyword;

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var breakKeyword = (SyntaxToken)visitor.Visit(BreakKeyword);
        return breakKeyword == BreakKeyword ? this : BreakStatement(breakKeyword);
    }
}
