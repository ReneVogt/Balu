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
        return node.Kind switch
        {
            SyntaxKind.EndOfFileToken => VisitEndOfFileToken(node),
            SyntaxKind.WhiteSpaceToken => VisitWhiteSpaceToken(node),
            SyntaxKind.BadToken  => VisitBadToken(node),
            SyntaxKind.NumberToken  => VisitNumberToken(node),
            SyntaxKind.PlusToken  => VisitPlusToken(node),
            SyntaxKind.MinusToken => VisitMinusToken(node),
            SyntaxKind.StarToken => VisitStarToken(node),
            SyntaxKind.SlashToken => VisitSlashToken(node),
            SyntaxKind.OpenParenthesisToken => VisitOpenParenthesisToken(node),
            SyntaxKind.ClosedParenthesisToken => VisitClosedParenthesisToken(node),
            SyntaxKind.OpenBraceToken => VisitOpenBraceToken(node),
            SyntaxKind.ClosedBraceToken => VisitClosedBraceToken(node),
            SyntaxKind.EqualsToken => VisitEqualsToken(node),
            SyntaxKind.BangToken => VisitBangToken(node),
            SyntaxKind.AmpersandAmpersandToken => VisitAmpersandAmpersandToken(node),
            SyntaxKind.PipePipeToken => VisitPipePipeToken(node),
            SyntaxKind.EqualsEqualsToken => VisitEqualsEqualsToken(node),
            SyntaxKind.BangEqualsToken => VisitBangEqualsToken(node),
            SyntaxKind.IdentifierToken => VisitIdentifierToken(node),
            SyntaxKind.LiteralExpression => VisitLiteralExpression((LiteralExpressionSyntax)node),
            SyntaxKind.UnaryExpression => VisitUnaryExpression((UnaryExpressionSyntax)node),
             SyntaxKind.BinaryExpression => VisitBinaryExpression((BinaryExpressionSyntax)node),
            SyntaxKind.ParenthesizedExpression => VisitParenthesizedExpression((ParenthesizedExpressionSyntax)node),
            SyntaxKind.NameExpression => VisitNameExpression((NameExpressionSyntax)node),
            SyntaxKind.AssignmentExpression => VisitAssignmentExpression((AssignmentExpressionSyntax)node),
            SyntaxKind.BlockStatement => VisitBlockStatement((BlockStatementSyntax)node),
            SyntaxKind.ExpressionStatement => VisitExpressionStatement((ExpressionStatementSyntax)node),
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
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.OpenBraceToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitOpenBraceToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.ClosedBraceToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitClosedBraceToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.EqualsToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitEqualsToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.BangToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBangToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitAmpersandAmpersandToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.PipePipeToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitPipePipeToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitEqualsEqualsToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.BangEqualsToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBangEqualsToken(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.IdentifierToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitIdentifierToken(SyntaxNode node) => node.Accept(this);

    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.TrueKeyword"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitTrueKeyword(SyntaxNode node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> of <see cref="SyntaxNode.Kind"/> <see cref="SyntaxKind.FalseKeyword"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitFalseKeyword(SyntaxNode node) => node.Accept(this);
    
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
    /// <summary>
    /// Visits a <see cref="NameExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="NameExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitNameExpression(NameExpressionSyntax node) => node.Accept(this);

    /// <summary>
    /// Visits a <see cref="AssignmentExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="AssignmentExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="BlockStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BlockStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBlockStatement(BlockStatementSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="ExpressionStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ExpressionStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node) => node.Accept(this);
}
