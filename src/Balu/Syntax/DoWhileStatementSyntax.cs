using System;

namespace Balu.Syntax;

public sealed partial class DoWhileStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
    public SyntaxToken DoKeyword { get; }
    public StatementSyntax Body { get; }
    public SyntaxToken WhileKeyword { get; }
    public ExpressionSyntax Condition { get; }

    internal DoWhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword,
                                    ExpressionSyntax condition)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        DoKeyword = doKeyword ?? throw new ArgumentNullException(nameof(doKeyword));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        WhileKeyword = whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }
}
