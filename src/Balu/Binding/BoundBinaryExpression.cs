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
}