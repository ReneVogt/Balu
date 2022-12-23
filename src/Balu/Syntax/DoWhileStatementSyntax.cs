using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class DoWhileStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
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
    public SyntaxToken DoKeyword { get; }
    public StatementSyntax Body { get; }
    public SyntaxToken WhileKeyword { get; }
    public ExpressionSyntax Condition { get; }

    public DoWhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword,
                                  ExpressionSyntax condition)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        DoKeyword = doKeyword ?? throw new ArgumentNullException(nameof(doKeyword));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        WhileKeyword = whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var doKeyword = (SyntaxToken)visitor.Visit(DoKeyword);
        var statement = (StatementSyntax)visitor.Visit(Body);
        var whileKeyword = (SyntaxToken)visitor.Visit(WhileKeyword);
        var condition = (ExpressionSyntax)visitor.Visit(Condition);
        return doKeyword == DoKeyword&& statement == Body && whileKeyword == WhileKeyword && condition == Condition
                   ? this
                   : new(SyntaxTree, doKeyword, statement, whileKeyword, condition);
    }
}
