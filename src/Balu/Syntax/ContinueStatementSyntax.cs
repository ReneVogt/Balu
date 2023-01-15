using System;

namespace Balu.Syntax;

public sealed partial class ContinueStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
    public SyntaxToken ContinueKeyword { get; }

    internal ContinueStatementSyntax(SyntaxTree syntaxTree, SyntaxToken continueKeyword)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => ContinueKeyword = continueKeyword ?? throw new ArgumentNullException(nameof(continueKeyword));

}
