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

    public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IfKeyword = ifKeyword ?? throw new ArgumentNullException(nameof(ifKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        ThenStatement = thenStatement ?? throw new ArgumentNullException(nameof(thenStatement));
        ElseClause = elseClause;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var ifKeyword = (SyntaxToken)visitor.Visit(IfKeyword);
        var condition = (ExpressionSyntax)visitor.Visit(Condition);
        var thenStatement = (StatementSyntax)visitor.Visit(ThenStatement);
        var elseClause = ElseClause is {} ? (ElseClauseSyntax)visitor.Visit(ElseClause) : null;
        return ifKeyword == IfKeyword && condition == Condition && thenStatement == ThenStatement && elseClause == ElseClause
                   ? this
                   : new(SyntaxTree, ifKeyword, condition, thenStatement, elseClause);
    }
}
