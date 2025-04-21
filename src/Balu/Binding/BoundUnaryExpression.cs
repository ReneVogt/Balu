using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundUnaryExpression(SyntaxNode syntax, BoundUnaryOperator op, BoundExpression operand) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; } = ConstantFolder.ComputeConstant(op, operand);
    public override bool HasSideEffects { get; } = operand.HasSideEffects;

    public BoundUnaryOperator Operator { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override string ToString() => $"{Operator.SyntaxKind.GetText()} {Operand}";
}