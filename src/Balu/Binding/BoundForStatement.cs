namespace Balu.Binding;

sealed class BoundForStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

    public VariableSymbol Variable { get; }
    public BoundExpression LowerBound { get; }
    public BoundExpression UpperBound { get; }
    public BoundStatement Body { get; }

    public BoundForStatement(VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound, BoundStatement body)
    {
        Variable = variable;
        LowerBound = lowerBound; 
        UpperBound = upperBound;
        Body = body;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var lowerBound = (BoundExpression)visitor.Visit(LowerBound);
        var upperBound = (BoundExpression)visitor.Visit(UpperBound);
        var body = (BoundStatement)visitor.Visit(Body);
        return lowerBound == LowerBound && upperBound == UpperBound && body == Body
                   ? this
                   : new (Variable, lowerBound, upperBound, body);
    }
}