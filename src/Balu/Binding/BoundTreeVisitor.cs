using System;

namespace Balu.Binding;

abstract class BoundTreeVisitor
{
    public virtual void Visit(BoundNode node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.LiteralExpression:
                VisitBoundLiteralExpression((BoundLiteralExpression)node);
                break;
            case BoundNodeKind.UnaryExpression:
                VisitBoundUnaryExpression((BoundUnaryExpression)node);
                break;
            case BoundNodeKind.BinaryExpression:
                VisitBoundBinaryExpression((BoundBinaryExpression)node);
                break;
            case BoundNodeKind.VariableExpression:
                VisitBoundVariableExpression((BoundVariableExpression)node);
                break;
            case BoundNodeKind.AssignmentExpression:
                VisitBoundAssignmentExpression((BoundAssignmentExpression)node);
                break;
            case BoundNodeKind.CallExpression:
                VisitBoundCallExpression((BoundCallExpression)node);
                break;
            case BoundNodeKind.ConversionExpression:
                VisitBoundConversionExpression((BoundConversionExpression)node);
                break;
            case BoundNodeKind.ErrorExpression:
                VisitBoundErrorExpression((BoundErrorExpression)node);
                break;
            case BoundNodeKind.BlockStatement:
                VisitBoundBlockStatement((BoundBlockStatement)node);
                break;
            case BoundNodeKind.ExpressionStatement:
                VisitBoundExpressionStatement((BoundExpressionStatement)node);
                break;
            case BoundNodeKind.VariableDeclarationStatement:
                VisitBoundVariableDeclarationStatement((BoundVariableDeclarationStatement)node);
                break;
            case BoundNodeKind.IfStatement:
                VisitBoundIfStatement((BoundIfStatement)node);
                break;
            case BoundNodeKind.WhileStatement:
                VisitBoundWhileStatement((BoundWhileStatement)node);
                break;
            case BoundNodeKind.DoWhileStatement:
                VisitBoundDoWhileStatement((BoundDoWhileStatement)node);
                break;
            case BoundNodeKind.ForStatement:
                VisitBoundForStatement((BoundForStatement)node);
                break;
            case BoundNodeKind.LabelStatement:
                VisitBoundLabelStatement((BoundLabelStatement)node);
                break;
            case BoundNodeKind.GotoStatement:
                VisitBoundGotoStatement((BoundGotoStatement)node);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                VisitBoundConditionalGotoStatement((BoundConditionalGotoStatement)node);
                break;
            case BoundNodeKind.ReturnStatement:
                VisitBoundReturnStatement((BoundReturnStatement)node);
                break;
            case BoundNodeKind.NopStatement:
                VisitBoundNopStatement((BoundNopStatement)node);
                break;
            default:
                throw new ArgumentException($"Unknown {nameof(BoundNodeKind)} '{node.Kind}'.");
        }
    }
    protected virtual void VisitBoundLiteralExpression(BoundLiteralExpression literalExpression) => literalExpression.Accept(this);
    protected virtual void VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression) => unaryExpression.Accept(this);
    protected virtual void VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression) => binaryExpression.Accept(this);
    protected virtual void VisitBoundVariableExpression(BoundVariableExpression variableExpression) => variableExpression.Accept(this);
    protected virtual void VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression) => assignmentExpression.Accept(this);
    protected virtual void VisitBoundCallExpression(BoundCallExpression callExpression) => callExpression.Accept(this);
    protected virtual void VisitBoundConversionExpression(BoundConversionExpression conversionExpression) => conversionExpression.Accept(this);
    protected virtual void VisitBoundErrorExpression(BoundErrorExpression errorExpression) => errorExpression.Accept(this);
    protected virtual void VisitBoundBlockStatement(BoundBlockStatement blockStatement) => blockStatement.Accept(this);
    protected virtual void VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => expressionStatement.Accept(this);
    protected virtual void VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement) => variableDeclarationStatement.Accept(this);
    protected virtual void VisitBoundIfStatement(BoundIfStatement ifStatement) => ifStatement.Accept(this);
    protected virtual void VisitBoundWhileStatement(BoundWhileStatement whileStatement) => whileStatement.Accept(this);
    protected virtual void VisitBoundDoWhileStatement(BoundDoWhileStatement doWhileStatement) => doWhileStatement.Accept(this);
    protected virtual void VisitBoundForStatement(BoundForStatement forStatement) => forStatement.Accept(this);
    protected virtual void VisitBoundLabelStatement(BoundLabelStatement labelStatement) => labelStatement.Accept(this);
    protected virtual void VisitBoundGotoStatement(BoundGotoStatement gotoStatement) => gotoStatement.Accept(this);
    protected virtual void VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement) => conditionalGotoStatement.Accept(this);
    protected virtual void VisitBoundReturnStatement(BoundReturnStatement returnStatement) => returnStatement.Accept(this);
    protected virtual void VisitBoundNopStatement(BoundNopStatement nopStatement) => nopStatement.Accept(this);
}
