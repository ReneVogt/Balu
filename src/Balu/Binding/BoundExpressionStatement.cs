namespace Balu.Binding;

sealed class BoundExpressionStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
    public BoundExpression Expression { get; }

    public BoundExpressionStatement(BoundExpression expression)
    {
        Expression = expression;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new BoundExpressionStatement(expression);
    }
}