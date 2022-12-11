using System;

namespace Balu.Binding;

abstract class BoundTreeVisitor
{
    public virtual BoundNode Visit(BoundNode node) => node.Kind switch
    {
        BoundNodeKind.LiteralExpression => VisitBoundLiteralExpression((BoundLiteralExpression)node),
        BoundNodeKind.UnaryExpression => VisitBoundUnaryExpression((BoundUnaryExpression)node),
        BoundNodeKind.BinaryExpression => VisitBoundBinaryExpression((BoundBinaryExpression)node),
        BoundNodeKind.VariableExpression => VisitBoundVariableExpression((BoundVariableExpression)node),
        BoundNodeKind.AssignmentExpression => VisitBoundAssignmentExpression((BoundAssignmentExpression)node),
        BoundNodeKind.BlockStatement => VisitBoundBlockStatement((BoundBlockStatement)node),
        BoundNodeKind.ExpressionStatement => VisitBoundExpressionStatement((BoundExpressionStatement)node),
        BoundNodeKind.VariableDeclarationStatement => VisitBoundVariableDeclarationStatement((BoundVariableDeclarationStatement)node),
        BoundNodeKind.IfStatement => VisitBoundIfStatement((BoundIfStatement)node),
        BoundNodeKind.WhileStatement => VisitBoundWhileStatement((BoundWhileStatement)node),
        BoundNodeKind.ForStatement => VisitBoundForStatement((BoundForStatement)node),
        BoundNodeKind.LabelStatement => VisitBoundLabelStatement((BoundLabelStatement)node),
        BoundNodeKind.GotoStatement => VisitBoundGotoStatement((BoundGotoStatement)node),
        BoundNodeKind.ConditionalGotoStatement => VisitBoundConditionalGotoStatement((BoundConditionalGotoStatement)node),
        _ => throw new ArgumentException($"Unknown {nameof(BoundNodeKind)} '{node.Kind}'.")
    };
    protected virtual BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression) => literalExpression.Accept(this);
    protected virtual BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression) => unaryExpression.Accept(this);
    protected virtual BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression) => binaryExpression.Accept(this);
    protected virtual BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression) => variableExpression.Accept(this);
    protected virtual BoundNode VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression) => assignmentExpression.Accept(this);
    protected virtual BoundNode VisitBoundBlockStatement(BoundBlockStatement blockStatement) => blockStatement.Accept(this);
    protected virtual BoundNode VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => expressionStatement.Accept(this);
    protected virtual BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement) => variableDeclarationStatement.Accept(this);
    protected virtual BoundNode VisitBoundIfStatement(BoundIfStatement ifStatemnet) => ifStatemnet.Accept(this);
    protected virtual BoundNode VisitBoundWhileStatement(BoundWhileStatement whileStatement) => whileStatement.Accept(this);
    protected virtual BoundNode VisitBoundForStatement(BoundForStatement forStatement) => forStatement.Accept(this);
    protected virtual BoundNode VisitBoundLabelStatement(BoundLabelStatement labelStatement) => labelStatement.Accept(this);
    protected virtual BoundNode VisitBoundGotoStatement(BoundGotoStatement gotoStatement) => gotoStatement.Accept(this);
    protected virtual BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement) => conditionalGotoStatement.Accept(this);
}
