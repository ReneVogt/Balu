namespace Balu.Binding;

sealed class BoundWhileStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
    public BoundExpression Condition { get; }
    public BoundStatement Body { get; }

    public BoundWhileStatement(BoundExpression condition, BoundStatement statement) => (Condition, Body) = (condition, statement);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var condition = (BoundExpression)visitor.Visit(Condition);
        var body = (BoundStatement)visitor.Visit(Body);
        return condition == Condition && body == Body
                   ? this
                   : new (condition, body);
    }
}