using System;

namespace Balu.Syntax;

public sealed partial class ExpressionStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    public ExpressionSyntax Expression { get; }

    internal ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax? expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));
}
