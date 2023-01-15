using System;

namespace Balu.Syntax;

public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

    internal AssignmentExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IdentifierToken = identifierrToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }
}
