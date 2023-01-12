using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class GlobalStatementSyntax : MemberSyntax
{
    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get { yield return Statement; }
    }

    public StatementSyntax Statement { get; }

    internal GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Statement = statement ?? throw new ArgumentNullException(nameof(statement)) ;
    }
}
