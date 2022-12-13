using Balu.Symbols;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Operand;
        }
    }

    public BoundUnaryOperator Operator { get; }
    public BoundExpression Operand { get; }

    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand) => (Operator, Operand) = (op, operand);

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var operand = (BoundExpression)visitor.Visit(Operand);
        return operand == Operand ? this : new (Operator, operand);
    }

    public override string ToString() => $"{Kind} {Operator.OperatorKind} ({Operand.Type.Name} => {Type.Name})";
}