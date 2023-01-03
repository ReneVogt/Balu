using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return IdentifierToken;
            yield return EqualsToken;
            yield return Expression;
        }
    }

    public AssignmentExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IdentifierToken = identifierrToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }

    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter)
    {
        SyntaxToken identifierToken = (SyntaxToken)rewriter.Visit(IdentifierToken);
        SyntaxToken equalsToken = (SyntaxToken)rewriter.Visit(EqualsToken);
        ExpressionSyntax expression = (ExpressionSyntax)rewriter.Visit(Expression);
        return identifierToken == IdentifierToken && equalsToken == EqualsToken && expression == Expression ? this : throw new NotImplementedException();
    }
}
