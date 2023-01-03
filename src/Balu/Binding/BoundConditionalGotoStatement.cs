using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundLabel Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Condition; }
    }
    
    public BoundConditionalGotoStatement(BoundLabel label, BoundExpression condition, bool jumpIfTrue = true) => (Label, Condition, JumpIfTrue) = (label, condition, jumpIfTrue);

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var condition = (BoundExpression)rewriter.Visit(Condition);
        return condition == Condition ? this : new (Label, condition, JumpIfTrue);
    }

    public override string ToString() => $"goto {Label} {(JumpIfTrue ? "if" : "if not")} {Condition}";
}
