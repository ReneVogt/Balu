using System;

namespace Balu.Syntax;

public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    internal UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax expression) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }
}
