using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.IfStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return IfKeyword;
            yield return Condition;
            yield return ThenStatement;
            if (ElseClause is { }) yield return ElseClause;
        }
    }
    public SyntaxToken IfKeyword { get; }
    public ExpressionSyntax Condition { get; }
    public StatementSyntax ThenStatement { get; }
    public ElseClauseSyntax? ElseClause { get; }

    internal IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IfKeyword = ifKeyword ?? throw new ArgumentNullException(nameof(ifKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        ThenStatement = thenStatement ?? throw new ArgumentNullException(nameof(thenStatement));
        ElseClause = elseClause;
    }
}
