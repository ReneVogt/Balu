﻿using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundWhileStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Condition;
            yield return Body;
        }
    }

    public BoundExpression Condition { get; }
    public BoundStatement Body { get; }

    public BoundWhileStatement(BoundExpression condition, BoundStatement body) => (Condition, Body) = (condition, body);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var condition = (BoundExpression)visitor.Visit(Condition);
        var body = (BoundStatement)visitor.Visit(Body);
        return condition == Condition && body == Body
                   ? this
                   : new (condition, body);
    }
}