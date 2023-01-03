using System;

namespace Balu.Syntax;

public abstract class SyntaxTreeRewriter
{
    public virtual SyntaxNode Visit(SyntaxNode node)
    {
        _ = node ?? throw new ArgumentNullException(nameof(node));
        return node.Kind switch
        {
            SyntaxKind.CompilationUnit => VisitCompilationUnit((CompilationUnitSyntax)node),
            SyntaxKind.GlobalStatement => VisitGlobalStatement((GlobalStatementSyntax)node),
            SyntaxKind.FunctionDeclaration => VisitFunctionDeclaration((FunctionDeclarationSyntax)node),
            SyntaxKind.ReturnStatement => VisitReturnStatement((ReturnStatementSyntax)node),
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
            _ => node is SyntaxToken token ? token.Rewrite(this) : throw new ArgumentException($"Unknown {nameof(SyntaxKind)} '{node.Kind}'.")
        };
    }

    protected virtual SyntaxNode VisitToken(SyntaxToken node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitFunctionDeclaration(FunctionDeclarationSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitReturnStatement(ReturnStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitParameter(ParameterSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitNameExpression(NameExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitCallExpression(CallExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitBlockStatement(BlockStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitIfStatement(IfStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitElseClause(ElseClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitWhileStatement(WhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitDoWhileStatement(DoWhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitForStatement(ForStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitContinueStatement(ContinueStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitBreakStatement(BreakStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
    protected virtual SyntaxNode VisitTypeClause(TypeClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Rewrite(this);
}
