using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundConditionalGotoStatement : BoundStatement
{
    public LabelSymbol Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfFalse { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Condition; }
    }
    
    public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfFalse = false) => (Label, Condition, JumpIfFalse) = (label, condition, jumpIfFalse);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var condition = (BoundExpression)visitor.Visit(Condition);
        return condition == Condition ? this : new (Label, condition, JumpIfFalse);
    }

    public override string ToString() => $"{Kind} ({(JumpIfFalse ? "on false" : "on true")}) => {Label}";
}
