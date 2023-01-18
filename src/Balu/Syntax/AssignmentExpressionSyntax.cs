using System;

namespace Balu.Syntax;

public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken AssignmentToken { get; }
    public ExpressionSyntax Expression { get; }
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

    internal AssignmentExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken, SyntaxToken assignmentToken, ExpressionSyntax expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        IdentifierToken = identifierrToken;
        AssignmentToken = assignmentToken;
        Expression = expression;
    }
}
