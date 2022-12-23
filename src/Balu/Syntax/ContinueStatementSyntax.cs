using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ContinueStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ContinueKeyword;
        }
    }
    public SyntaxToken ContinueKeyword { get; }

    public ContinueStatementSyntax(SyntaxTree syntaxTree, SyntaxToken continueKeyword)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => ContinueKeyword = continueKeyword ?? throw new ArgumentNullException(nameof(continueKeyword));

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var continueKeyword = (SyntaxToken)visitor.Visit(ContinueKeyword);
        return continueKeyword == ContinueKeyword ? this : new(SyntaxTree, continueKeyword);
    }
}
