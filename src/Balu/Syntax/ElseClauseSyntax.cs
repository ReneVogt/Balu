using System;

namespace Balu.Syntax;

public sealed partial class ElseClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseClause;
    public SyntaxToken ElseKeyword { get; }
    public StatementSyntax Statement { get; }

    internal ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax statement)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ElseKeyword = elseKeyword ?? throw new ArgumentNullException(nameof(elseKeyword));
        Statement = statement ?? throw new ArgumentNullException(nameof(statement));
    }
}
