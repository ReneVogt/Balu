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

    internal WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body)
    :base(syntaxTree)
    {
        WhileKeyword = whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}
