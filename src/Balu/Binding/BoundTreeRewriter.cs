using System;

namespace Balu.Binding;

abstract class BoundTreeRewriter
{
    public virtual BoundNode Visit(BoundNode node) => node.Kind switch
    {
        BoundNodeKind.LiteralExpression => VisitBoundLiteralExpression((BoundLiteralExpression)node),
        BoundNodeKind.UnaryExpression => VisitBoundUnaryExpression((BoundUnaryExpression)node),
        BoundNodeKind.BinaryExpression => VisitBoundBinaryExpression((BoundBinaryExpression)node),
        BoundNodeKind.VariableExpression => VisitBoundVariableExpression((BoundVariableExpression)node),
        BoundNodeKind.AssignmentExpression => VisitBoundAssignmentExpression((BoundAssignmentExpression)node),
        BoundNodeKind.CallExpression => VisitBoundCallExpression((BoundCallExpression)node),
        BoundNodeKind.ConversionExpression => VisitBoundConversionExpression((BoundConversionExpression)node),
        BoundNodeKind.ErrorExpression => VisitBoundErrorExpression((BoundErrorExpression)node),
        BoundNodeKind.BlockStatement => VisitBoundBlockStatement((BoundBlockStatement)node),
        BoundNodeKind.ExpressionStatement => VisitBoundExpressionStatement((BoundExpressionStatement)node),
        BoundNodeKind.VariableDeclarationStatement => VisitBoundVariableDeclarationStatement((BoundVariableDeclarationStatement)node),
        BoundNodeKind.IfStatement => VisitBoundIfStatement((BoundIfStatement)node),
        BoundNodeKind.WhileStatement => VisitBoundWhileStatement((BoundWhileStatement)node),
        BoundNodeKind.DoWhileStatement => VisitBoundDoWhileStatement((BoundDoWhileStatement)node),
        BoundNodeKind.ForStatement => VisitBoundForStatement((BoundForStatement)node),
        BoundNodeKind.LabelStatement => VisitBoundLabelStatement((BoundLabelStatement)node),
        BoundNodeKind.GotoStatement => VisitBoundGotoStatement((BoundGotoStatement)node),
        BoundNodeKind.ConditionalGotoStatement => VisitBoundConditionalGotoStatement((BoundConditionalGotoStatement)node),
        BoundNodeKind.ReturnStatement => VisitBoundReturnStatement((BoundReturnStatement)node),
        BoundNodeKind.NopStatement => VisitBoundNopStatement((BoundNopStatement)node),
        BoundNodeKind.SequencePointStatement => VisitBoundSequencePointStatement((BoundSequencePointStatement)node),
        _ => throw new ArgumentException($"Unknown {nameof(BoundNodeKind)} '{node.Kind}'.")
    };
    protected virtual BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression) => literalExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression) => unaryExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression) => binaryExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression) => variableExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression) => assignmentExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundCallExpression(BoundCallExpression callExpression) => callExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundConversionExpression(BoundConversionExpression conversionExpression) => conversionExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundErrorExpression(BoundErrorExpression errorExpression) => errorExpression.Rewrite(this);
    protected virtual BoundNode VisitBoundBlockStatement(BoundBlockStatement blockStatement) => blockStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => expressionStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement) => variableDeclarationStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundIfStatement(BoundIfStatement ifStatement) => ifStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundWhileStatement(BoundWhileStatement whileStatement) => whileStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundDoWhileStatement(BoundDoWhileStatement doWhileStatement) => doWhileStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundForStatement(BoundForStatement forStatement) => forStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundLabelStatement(BoundLabelStatement labelStatement) => labelStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundGotoStatement(BoundGotoStatement gotoStatement) => gotoStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement) => conditionalGotoStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundReturnStatement(BoundReturnStatement returnStatement) => returnStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundNopStatement(BoundNopStatement nopStatement) => nopStatement.Rewrite(this);
    protected virtual BoundNode VisitBoundSequencePointStatement(BoundSequencePointStatement sequencePointStatement) => sequencePointStatement.Rewrite(this);
}
