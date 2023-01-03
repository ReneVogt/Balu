using System;

namespace Balu.Syntax;

public abstract class SyntaxTreeVisitor
{
    public virtual void Visit(SyntaxNode node)
    {
        _ = node ?? throw new ArgumentNullException(nameof(node));
        switch (node.Kind)
        {
            case SyntaxKind.CompilationUnit:
                VisitCompilationUnit((CompilationUnitSyntax)node);
                break; 
            case SyntaxKind.GlobalStatement:
                VisitGlobalStatement((GlobalStatementSyntax)node);
                break;
            case SyntaxKind.FunctionDeclaration:
                VisitFunctionDeclaration((FunctionDeclarationSyntax)node);
                break;
            case SyntaxKind.ReturnStatement:
                VisitReturnStatement((ReturnStatementSyntax)node);
                break;
            case SyntaxKind.Parameter:
                VisitParameter((ParameterSyntax)node);
                break;
            case SyntaxKind.LiteralExpression:
                VisitLiteralExpression((LiteralExpressionSyntax)node);
                break;
            case SyntaxKind.UnaryExpression:
                VisitUnaryExpression((UnaryExpressionSyntax)node);
                break;
            case SyntaxKind.BinaryExpression:
                VisitBinaryExpression((BinaryExpressionSyntax)node);
                break;
            case SyntaxKind.ParenthesizedExpression:
                VisitParenthesizedExpression((ParenthesizedExpressionSyntax)node);
                break;
            case SyntaxKind.NameExpression:
                VisitNameExpression((NameExpressionSyntax)node);
                break;
            case SyntaxKind.AssignmentExpression:
                VisitAssignmentExpression((AssignmentExpressionSyntax)node);
                break;
            case SyntaxKind.CallExpression:
                VisitCallExpression((CallExpressionSyntax)node);
                break;
            case SyntaxKind.BlockStatement:
                VisitBlockStatement((BlockStatementSyntax)node);
                break;
            case SyntaxKind.ExpressionStatement:
                VisitExpressionStatement((ExpressionStatementSyntax)node);
                break;
            case SyntaxKind.VariableDeclarationStatement:
                VisitVariableDeclarationStatement((VariableDeclarationStatementSyntax)node);
                break;
            case SyntaxKind.IfStatement:
                VisitIfStatement((IfStatementSyntax)node);
                break;
            case SyntaxKind.ElseClause:
                VisitElseClause((ElseClauseSyntax)node);
                break;
            case SyntaxKind.WhileStatement:
                VisitWhileStatement((WhileStatementSyntax)node);
                break;
            case SyntaxKind.DoWhileStatement:
                VisitDoWhileStatement((DoWhileStatementSyntax)node);
                break;
            case SyntaxKind.ForStatement:
                VisitForStatement((ForStatementSyntax)node);
                break;
            case SyntaxKind.ContinueStatement:
                VisitContinueStatement((ContinueStatementSyntax)node);
                break;
            case SyntaxKind.BreakStatement:
                VisitBreakStatement((BreakStatementSyntax)node);
                break;
            case SyntaxKind.TypeClause:
                VisitTypeClause((TypeClauseSyntax)node);
                break;
            default:
                (node as SyntaxToken ??  throw new ArgumentException($"Unknown {nameof(SyntaxKind)} '{node.Kind}'.")).Accept(this);
                break;
        }
    }

    protected virtual void VisitToken(SyntaxToken node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitCompilationUnit(CompilationUnitSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitGlobalStatement(GlobalStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitFunctionDeclaration(FunctionDeclarationSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitReturnStatement(ReturnStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitParameter(ParameterSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitLiteralExpression(LiteralExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitUnaryExpression(UnaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitBinaryExpression(BinaryExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitNameExpression(NameExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitAssignmentExpression(AssignmentExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitCallExpression(CallExpressionSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitBlockStatement(BlockStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitExpressionStatement(ExpressionStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitIfStatement(IfStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitElseClause(ElseClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitWhileStatement(WhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitDoWhileStatement(DoWhileStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitForStatement(ForStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitContinueStatement(ContinueStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitBreakStatement(BreakStatementSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
    protected virtual void VisitTypeClause(TypeClauseSyntax node) => (node ?? throw new ArgumentNullException(nameof(node))).Accept(this);
}
