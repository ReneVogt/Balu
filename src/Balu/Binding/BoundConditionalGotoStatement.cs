﻿using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundConditionalGotoStatement : BoundStatement
{
    public LabelSymbol Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Condition; }
    }
    
    public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue = true) => (Label, Condition, JumpIfTrue) = (label, condition, jumpIfTrue);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var condition = (BoundExpression)visitor.Visit(Condition);
        return condition == Condition ? this : new (Label, condition, JumpIfTrue);
    }

    public override string ToString() => $"{Kind} ({(JumpIfTrue ? "on true" : "on false")}) => {Label}";
}
