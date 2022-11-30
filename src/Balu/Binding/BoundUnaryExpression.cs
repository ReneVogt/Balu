using System;

namespace Balu.Binding;

sealed class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override Type Type => Operand.Type;

    public BoundUnaryOperatorKind OperatorKind { get; }
    public BoundExpression Operand { get; }

    public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand) => (OperatorKind, Operand) = (operatorKind, operand);
}