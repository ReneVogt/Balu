using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

public sealed class BlockStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
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

    public BlockStatementSyntax(SyntaxTree? syntaxTree, SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closedBraceToken)
    : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken ?? throw new ArgumentNullException(nameof(openBraceToken));
        Statements = statements;
        ClosedBraceToken = closedBraceToken ?? throw new ArgumentNullException(nameof(closedBraceToken));
    }


    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var openBrace = (SyntaxToken)visitor.Visit(OpenBraceToken);
        var transformed = VisitList(visitor, Statements);
        var closedBrace = (SyntaxToken)visitor.Visit(ClosedBraceToken);
        return openBrace == OpenBraceToken && transformed == Statements && closedBrace == ClosedBraceToken
                   ? this
                   : new(null, openBrace, transformed, closedBrace);
    }
}
