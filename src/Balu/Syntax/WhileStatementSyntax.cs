using System;
using System.Collections.Generic;

namespace Balu.Syntax;
public sealed class WhileStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return WhileKeyword;
            yield return Condition;
            yield return Body;
        }
    }
    public SyntaxToken WhileKeyword { get; }
    public ExpressionSyntax Condition { get; }
    public StatementSyntax Body { get; }

    public WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body)
    :base(syntaxTree)
    {
        WhileKeyword = whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var whileKeyword = (SyntaxToken)visitor.Visit(WhileKeyword);
        var condition = (ExpressionSyntax)visitor.Visit(Condition);
        var statement = (StatementSyntax)visitor.Visit(Body);
        return whileKeyword == WhileKeyword && condition == Condition && statement == Body
                   ? this
                   : new(SyntaxTree, whileKeyword, condition, statement);
    }
}
