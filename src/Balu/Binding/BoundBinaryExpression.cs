using System;

namespace Balu.Binding;

sealed class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override Type Type => Left.Type;

    public BoundExpression Left { get; }
    public BoundBinaryOperatorKind OperatorKind { get; }
    public BoundExpression Right { get; }
    
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
    {
        Left = left;
        OperatorKind = operatorKind;
        Right = right;
    }

    internal override BoundExpression Accept(BoundExpressionVisitor visitor)
    {
        var left = visitor.Visit(Left);
        var right = visitor.Visit(Right);
        return left == Left && right == Right ? this : new BoundBinaryExpression(left, OperatorKind, right);
    }
}