using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseClause;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ElseKeyword;
            yield return Statement;
        }
    }
    public SyntaxToken ElseKeyword { get; }
    public StatementSyntax Statement { get; }

    public ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax statement)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ElseKeyword = elseKeyword ?? throw new ArgumentNullException(nameof(elseKeyword));
        Statement = statement ?? throw new ArgumentNullException(nameof(statement));
    }
}
