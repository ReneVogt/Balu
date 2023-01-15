using System;

namespace Balu.Syntax;

public sealed partial class ReturnStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

    public SyntaxToken ReturnKeyword { get; }
    public ExpressionSyntax? Expression { get; }

    internal ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ReturnKeyword = returnKeyword ?? throw new ArgumentNullException(nameof(returnKeyword));
        Expression = expression;
    }
}
