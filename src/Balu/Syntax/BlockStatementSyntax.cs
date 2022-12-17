using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

/// <summary>
/// A block statement surrounded by braces.
/// </summary>
public sealed class BlockStatementSyntax : StatementSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenBraceToken;
            foreach (var statement in Statements) yield return statement;
            yield return ClosedBraceToken;
        }
    }

    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<StatementSyntax> Statements { get; }
    public SyntaxToken ClosedBraceToken { get; }

    internal BlockStatementSyntax(SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closedBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        ClosedBraceToken = closedBraceToken;
    }


    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var openBrace = (SyntaxToken)visitor.Visit(OpenBraceToken);
        var transformed = VisitList(visitor, Statements);
        var closedBrace = (SyntaxToken)visitor.Visit(ClosedBraceToken);
        return openBrace == OpenBraceToken && transformed == Statements && closedBrace == ClosedBraceToken
                   ? this
                   : BlockStatement(openBrace, transformed, closedBrace);
    }
}
