using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundUnaryExpression(SyntaxNode syntax, BoundUnaryOperator @operator, BoundExpression operand) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; } = ConstantFolder.ComputeConstant(@operator, operand);
    public override bool HasSideEffects { get; } = operand.HasSideEffects;

    public BoundUnaryOperator Operator { get; } = @operator;
    public BoundExpression Operand { get; } = operand;

    public override string ToString() => $"{Operator.SyntaxKind.GetText()} {Operand}";
}
