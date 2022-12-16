using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'while' statement.
/// </summary>
public sealed class DoWhileStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return DoKeyword;
            yield return Body;
            yield return WhileKeyword;
            yield return Condition;
        }
    }

    /// <summary>
    /// The 'do' keyword of the 'do...while' statement.
    /// </summary>
    public SyntaxToken DoKeyword { get; }
    /// <summary>
    /// The body of the 'do...while' statement.
    /// </summary>
    public StatementSyntax Body { get; }
    /// <summary>
    /// The <see cref="SyntaxToken"/> representing the 'do...while' keyword.
    /// </summary>
    public SyntaxToken WhileKeyword { get; }
    /// <summary>
    /// The 'do...while' condition.
    /// </summary>
    public ExpressionSyntax Condition { get; }

    internal DoWhileStatementSyntax(SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword, ExpressionSyntax condition)
    {
        DoKeyword = doKeyword;
        Body = body;
        WhileKeyword = whileKeyword;
        Condition = condition;
    }
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var doKeyword = (SyntaxToken)visitor.Visit(DoKeyword);
        var statement = (StatementSyntax)visitor.Visit(Body);
        var whileKeyword = (SyntaxToken)visitor.Visit(WhileKeyword);
        var condition = (ExpressionSyntax)visitor.Visit(Condition);
        return doKeyword == DoKeyword&& statement == Body && whileKeyword == WhileKeyword && condition == Condition
                   ? this
                   : DoWhileStatement(doKeyword, statement, whileKeyword, condition);
    }
}
