namespace Balu.Binding;

sealed class BoundWhileStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
    public BoundExpression Condition { get; }
    public BoundStatement Statement { get; }

    public BoundWhileStatement(BoundExpression condition, BoundStatement statement) => (Condition, Statement) = (condition, statement);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var condition = (BoundExpression)visitor.Visit(Condition);
        var statement = (BoundStatement)visitor.Visit(Statement);
        return condition == Condition && statement == Statement
                   ? this
                   : new (condition, statement);
    }
}