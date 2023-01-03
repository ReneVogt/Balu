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

    public BlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closedBraceToken)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OpenBraceToken = openBraceToken ?? throw new ArgumentNullException(nameof(openBraceToken));
        Statements = statements;
        ClosedBraceToken = closedBraceToken ?? throw new ArgumentNullException(nameof(closedBraceToken));
    }


    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter)
    {
        var openBrace = (SyntaxToken)rewriter.Visit(OpenBraceToken);
        var transformed = RewriteList(rewriter, Statements);
        var closedBrace = (SyntaxToken)rewriter.Visit(ClosedBraceToken);
        return openBrace == OpenBraceToken && transformed == Statements && closedBrace == ClosedBraceToken
                   ? this
                   : throw new NotImplementedException();
    }
}
