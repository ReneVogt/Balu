namespace Balu.Syntax;

/// <summary>
/// The abstract base class for <see cref="SyntaxNode"/> visitors.
/// </summary>
public abstract class SyntaxVisitor
{
    /// <summary>
    /// Constructs a <see cref="SyntaxVisitor"/> instance.
    /// </summary>
    protected SyntaxVisitor(){}

    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> by calling the correct method for the node's <see cref="SyntaxKind"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
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

    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.EndOfFileToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitEndOfFileToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitWhiteSpaceToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.BadToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBadToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.NumberToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitNumberToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.PlusToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitPlusToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.MinusToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitMinusToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.StarToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitStarToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.SlashToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitSlashToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitOpenParenthesisToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitClosedParenthesisToken(SyntaxNode node) => node.Accept(this);

    /// <summary>
    /// Visits a <see cref="LiteralExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="LiteralExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="UnaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="UnaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="BinaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BinaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ParenthesizedExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitParenthesizedExpression(SyntaxNode node) => node.Accept(this);
}
