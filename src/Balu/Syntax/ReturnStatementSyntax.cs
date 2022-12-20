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
        }
    }

    public SyntaxToken ReturnKeyword { get; }

    internal ReturnStatementSyntax(SyntaxToken returnKeyword) => ReturnKeyword = returnKeyword;

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var returnKeyword = (SyntaxToken)visitor.Visit(ReturnKeyword);
        return returnKeyword == ReturnKeyword ? this : ReturnStatement(returnKeyword);
    }
}
