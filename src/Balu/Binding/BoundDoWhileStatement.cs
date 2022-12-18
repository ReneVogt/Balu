using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundDoWhileStatement : BoundLoopStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Body;
            yield return Condition;
        }
    }

    public BoundStatement Body { get; }
    public BoundExpression Condition { get; }

    public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel) : base(breakLabel, continueLabel)
    {
        Body = body;
        Condition = condition;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var body = (BoundStatement)visitor.Visit(Body);
        var condition = (BoundExpression)visitor.Visit(Condition);
        return body == Body && condition == Condition
                   ? this
                   : new (body, condition, BreakLabel, ContinueLabel);
    }
}