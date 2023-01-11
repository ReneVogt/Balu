using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return OperatorToken;
            yield return Expression;
        }
    }

    public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax expression) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }
}
