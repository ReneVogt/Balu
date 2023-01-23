namespace Balu.Syntax;

public sealed partial class PrefixExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.PrefixExpression;
    public SyntaxToken OperatorToken { get; set; }
    public SyntaxToken IdentifierToken { get; set; }
    internal PrefixExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, SyntaxToken identifierToken) : base(syntaxTree)
    {
        OperatorToken = operatorToken;
        IdentifierToken = identifierToken;
    }
}
