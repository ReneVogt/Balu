using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents an 'if' statement.
/// </summary>
public sealed class IfStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.IfStatement;
    /// <inheritdoc/>
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

    /// <summary>
    /// The <see cref="SyntaxToken"/> representing the 'if' keyword.
    /// </summary>
    public SyntaxToken IfKeyword { get; }
    /// <summary>
    /// The 'if' condition.
    /// </summary>
    public ExpressionSyntax Condition { get; }
    /// <summary>
    /// The statement if the <see cref="Condition"/> is true.
    /// </summary>
    public StatementSyntax ThenStatement { get; }
    /// <summary>
    /// The optional 'else' part.
    /// </summary>
    public ElseClauseSyntax? ElseClause { get; }

    internal IfStatementSyntax(SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause)
    {
        IfKeyword = ifKeyword;
        Condition = condition;
        ThenStatement = thenStatement;
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
                   : IfStatement(ifKeyword, condition, thenStatement, elseClause);
    }
}
