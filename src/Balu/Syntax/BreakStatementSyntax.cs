using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class BreakStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return BreakKeyword;
        }
    }
    public SyntaxToken BreakKeyword { get; }

    public BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken breakKeyword)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => BreakKeyword = breakKeyword ?? throw new ArgumentNullException(nameof(breakKeyword));

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var breakKeyword = (SyntaxToken)visitor.Visit(BreakKeyword);
        return breakKeyword == BreakKeyword ? this : new(SyntaxTree, breakKeyword);
    }
}
