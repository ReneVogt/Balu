using System;

namespace Balu.Binding;

sealed class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override Type Type => Operator.ResultType;

    public BoundUnaryOperator Operator { get; }
    public BoundExpression Operand { get; }

    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand) => (Operator, Operand) = (op, operand);

    internal override BoundExpression Accept(BoundExpressionVisitor visitor)
    {
        var operand = visitor.Visit(Operand);
        return operand == Operand ? this : new (Operator, operand);
    }

}