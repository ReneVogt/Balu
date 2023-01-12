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

    internal AssignmentExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IdentifierToken = identifierrToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }
}
