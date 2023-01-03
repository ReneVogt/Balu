using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken ClosedParenthesisToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return ClosedParenthesisToken;
        }
    }

    public ParenthesizedExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closedParenthesisToken)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OpenParenthesisToken = openParenthesisToken ?? throw new ArgumentNullException(nameof(openParenthesisToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        ClosedParenthesisToken = closedParenthesisToken ?? throw new ArgumentNullException(nameof(closedParenthesisToken));
    }

    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter)
    {
        SyntaxToken open = (SyntaxToken)rewriter.Visit(OpenParenthesisToken);
        ExpressionSyntax expr = (ExpressionSyntax)rewriter.Visit(Expression);
        SyntaxToken close = (SyntaxToken)rewriter.Visit(ClosedParenthesisToken);
        return open == OpenParenthesisToken && expr == Expression && close == ClosedParenthesisToken ? this : throw new NotImplementedException();
    }

}
