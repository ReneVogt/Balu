namespace Balu.Binding;

abstract class BoundExpressionVisitor
{
    public virtual BoundExpression Visit(BoundExpression expression) => expression switch
    {
        BoundLiteralExpression literalExpression => VisitBoundLiteralExpression(literalExpression),
        BoundUnaryExpression unaryExpression => VisitBoundUnaryExpression(unaryExpression),
        BoundBinaryExpression binaryExpression => VisitBoundBinaryExpression(binaryExpression),
        _ => expression
    };
    protected virtual BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression literalExpression) => literalExpression.Accept(this);
    protected virtual BoundExpression VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression) => unaryExpression.Accept(this);
    protected virtual BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression) => binaryExpression.Accept(this);
}
