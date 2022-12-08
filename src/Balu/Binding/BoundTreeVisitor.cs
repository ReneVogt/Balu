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
        BoundNodeKind.VariableDeclaration => VisitBoundVariableDeclarationStatement((BoundVariableDeclaration)node),
        _ => node
    };
    protected virtual BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression) => literalExpression.Accept(this);
    protected virtual BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression) => unaryExpression.Accept(this);
    protected virtual BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression) => binaryExpression.Accept(this);
    protected virtual BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression) => variableExpression.Accept(this);
    protected virtual BoundNode VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression) => assignmentExpression.Accept(this);
    protected virtual BoundNode VisitBoundBlockStatement(BoundBlockStatement blockStatement) => blockStatement.Accept(this);
    protected virtual BoundNode VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement) => expressionStatement.Accept(this);
    protected virtual BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclaration variableDeclaration) => variableDeclaration.Accept(this);
}
