namespace Balu.Syntax;

public class SyntaxVisitor
{
    public virtual SyntaxNode Visit(SyntaxNode node)
    {
        return node switch
        {
            { Kind: SyntaxKind.EndOfFileToken } => VisitEndOfFileToken(node),
            { Kind: SyntaxKind.WhiteSpaceToken } => VisitWhiteSpaceToken(node),
            { Kind: SyntaxKind.BadToken } => VisitBadToken(node),
            { Kind: SyntaxKind.NumberToken } => VisitNumberToken(node),
            { Kind: SyntaxKind.PlusToken } => VisitPlusToken(node),
            { Kind: SyntaxKind.MinusToken } => VisitMinusToken(node),
            { Kind: SyntaxKind.StarToken } => VisitStarToken(node),
            { Kind: SyntaxKind.SlashToken } => VisitSlashToken(node),
            { Kind: SyntaxKind.OpenParenthesisToken } => VisitOpenParenthesisToken(node),
            { Kind: SyntaxKind.ClosedParenthesisToken } => VisitClosedParenthesisToken(node),
            LiteralExpressionSyntax literal => VisitLiteralExpression(literal),
            UnaryExpressionSyntax unary => VisitUnaryExpression(unary),
            BinaryExpressionSyntax binary => VisitBinaryExpression(binary),
            ParenthesizedExpressionSyntax parenthesized => VisitParenthesizedExpression(parenthesized),
            _ => node.Accept(this)
        };
    }

    public virtual SyntaxNode VisitEndOfFileToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitWhiteSpaceToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitBadToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitNumberToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitPlusToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitMinusToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitStarToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitSlashToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitOpenParenthesisToken(SyntaxNode node) => node.Accept(this);
    private SyntaxNode VisitClosedParenthesisToken(SyntaxNode node) => node.Accept(this);

    private SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node) => node.Accept(this);
    private SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node) => node.Accept(this);
    private SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => node.Accept(this);
    private SyntaxNode VisitParenthesizedExpression(SyntaxNode node) => node.Accept(this);

}
