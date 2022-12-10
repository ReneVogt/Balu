using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override Type Type => Operator.Type;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Left;
            yield return Right;
        }
    }

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }
    
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var left = (BoundExpression)visitor.Visit(Left);
        var right = (BoundExpression)visitor.Visit(Right);
        return left == Left && right == Right ? this : new (left, Operator, right);
    }
}