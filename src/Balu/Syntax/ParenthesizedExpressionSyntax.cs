using System;

namespace Balu.Syntax;

public sealed partial class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken ClosedParenthesisToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

    internal ParenthesizedExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closedParenthesisToken)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OpenParenthesisToken = openParenthesisToken ?? throw new ArgumentNullException(nameof(openParenthesisToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        ClosedParenthesisToken = closedParenthesisToken ?? throw new ArgumentNullException(nameof(closedParenthesisToken));
    }
}
