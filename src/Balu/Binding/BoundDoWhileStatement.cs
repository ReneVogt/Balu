﻿using System.Collections.Generic;

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

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var body = (BoundStatement)rewriter.Visit(Body);
        var condition = (BoundExpression)rewriter.Visit(Condition);
        return body == Body && condition == Condition
                   ? this
                   : new (body, condition, BreakLabel, ContinueLabel);
    }
}