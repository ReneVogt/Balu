namespace Balu.Syntax;

public sealed partial class PostfixExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PostfixExpression;
    public SyntaxToken IdentifierToken { get; set; }
    public SyntaxToken OperatorToken { get; set; }
    internal PostfixExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken operatorToken) : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        OperatorToken = operatorToken;
    }
}
