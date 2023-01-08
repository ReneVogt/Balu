using Balu.Symbols;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; }

    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Operand;
        }
    }

    public BoundUnaryOperator Operator { get; }
    public BoundExpression Operand { get; }

    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
    {
        Operator = op;
        Operand = operand;
        Constant = ConstantFolder.ComputeConstant(op, operand);
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var operand = (BoundExpression)rewriter.Visit(Operand);
        return operand == Operand ? this : new (Operator, operand);
    }

    public override string ToString() => $"{Operator.SyntaxKind.GetText()} {Operand}";
}