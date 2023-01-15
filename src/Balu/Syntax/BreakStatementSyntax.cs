using System;

namespace Balu.Syntax;

public sealed partial class BreakStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
    public SyntaxToken BreakKeyword { get; }

    internal BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken breakKeyword)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => BreakKeyword = breakKeyword ?? throw new ArgumentNullException(nameof(breakKeyword));
}
