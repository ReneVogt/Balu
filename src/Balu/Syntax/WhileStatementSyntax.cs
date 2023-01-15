using System;

namespace Balu.Syntax;
public sealed partial class WhileStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
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
