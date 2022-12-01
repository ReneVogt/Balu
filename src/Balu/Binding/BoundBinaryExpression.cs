using System;

namespace Balu.Binding;

sealed class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override Type Type => Operator.ResultType;

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }
    
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    internal override BoundExpression Accept(BoundExpressionVisitor visitor)
    {
        var left = visitor.Visit(Left);
        var right = visitor.Visit(Right);
        return left == Left && right == Right ? this : new (left, Operator, right);
    }
}