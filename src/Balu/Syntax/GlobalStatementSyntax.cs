using System;

namespace Balu.Syntax;

public sealed partial class GlobalStatementSyntax : MemberSyntax
{
    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    public StatementSyntax Statement { get; }

    internal GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Statement = statement ?? throw new ArgumentNullException(nameof(statement)) ;
    }
}
