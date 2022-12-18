using System;

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
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    public virtual SyntaxNode Visit(SyntaxNode node)
    {
        _ = node ?? throw new ArgumentNullException(nameof(node));
        return node.Kind switch
        {
            SyntaxKind.CompilationUnit => VisitCompilationUnit((CompilationUnitSyntax)node),
            SyntaxKind.GlobalStatement => VisitGlobalStatement((GlobalStatementSyntax)node),
            SyntaxKind.FunctionDeclaration => VisitFunctionDeclaration((FunctionDeclarationSyntax)node),
            SyntaxKind.Parameter => VisitParameter((ParameterSyntax)node),
            SyntaxKind.LiteralExpression => VisitLiteralExpression((LiteralExpressionSyntax)node),
            SyntaxKind.UnaryExpression => VisitUnaryExpression((UnaryExpressionSyntax)node),
            SyntaxKind.BinaryExpression=> VisitBinaryExpression((BinaryExpressionSyntax)node),
            SyntaxKind.ParenthesizedExpression => VisitParenthesizedExpression((ParenthesizedExpressionSyntax)node),
            SyntaxKind.NameExpression => VisitNameExpression((NameExpressionSyntax)node),
            SyntaxKind.AssignmentExpression => VisitAssignmentExpression((AssignmentExpressionSyntax)node),
            SyntaxKind.CallExpression => VisitCallExpression((CallExpressionSyntax)node),
            SyntaxKind.BlockStatement => VisitBlockStatement((BlockStatementSyntax)node),
            SyntaxKind.ExpressionStatement => VisitExpressionStatement((ExpressionStatementSyntax)node),
            SyntaxKind.VariableDeclarationStatement => VisitVariableDeclarationStatement((VariableDeclarationStatementSyntax)node),
            SyntaxKind.IfStatement => VisitIfStatement((IfStatementSyntax)node),
            SyntaxKind.ElseClause => VisitElseClause((ElseClauseSyntax)node),
            SyntaxKind.WhileStatement => VisitWhileStatement((WhileStatementSyntax)node),
            SyntaxKind.DoWhileStatement => VisitDoWhileStatement((DoWhileStatementSyntax)node),
            SyntaxKind.ForStatement => VisitForStatement((ForStatementSyntax)node),
            SyntaxKind.ContinueStatement => VisitContinueStatement((ContinueStatementSyntax)node),
            SyntaxKind.BreakStatement => VisitBreakStatement((BreakStatementSyntax)node),
            SyntaxKind.TypeClause => VisitTypeClause((TypeClauseSyntax)node),
            _ => node is SyntaxToken token ? token.Accept(this) : throw new ArgumentException($"Unknown {nameof(SyntaxKind)} '{node.Kind}'.")
        };
    }

    /// <summary>
    /// Visits a <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxToken"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxToken"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitToken(SyntaxToken node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="CompilationUnitSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="CompilationUnitSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="GlobalStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="GlobalStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="FunctionDeclarationSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="FunctionDeclarationSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitFunctionDeclaration(FunctionDeclarationSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ParameterSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ParameterSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitParameter(ParameterSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="LiteralExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="LiteralExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="UnaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="UnaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="BinaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BinaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ParenthesizedExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="NameExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="NameExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitNameExpression(NameExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="AssignmentExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="AssignmentExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="CallExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="CallExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitCallExpression(CallExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);

    /// <summary>
    /// Visits a <see cref="BlockStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BlockStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitBlockStatement(BlockStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ExpressionStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ExpressionStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="VariableDeclarationStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="VariableDeclarationStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="IfStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="IfStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitIfStatement(IfStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ElseClauseSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ElseClauseSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitElseClause(ElseClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="WhileStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="WhileStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitWhileStatement(WhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="DoWhileStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="DoWhileStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitDoWhileStatement(DoWhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ForStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ForStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitForStatement(ForStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="ContinueStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ContinueStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitContinueStatement(ContinueStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="BreakStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BreakStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitBreakStatement(BreakStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    /// <summary>
    /// Visits a <see cref="TypeClauseSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="TypeClauseSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
    protected virtual SyntaxNode VisitTypeClause(TypeClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
}
