using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundUnaryExpression(SyntaxNode syntax, BoundUnaryOperator @operator, BoundExpression operand) : BoundExpression(syntax)
{
    readonly ConstantFoldingResult foldingResult = ConstantFolder.Fold(@operator, operand);

    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant => foldingResult.Constant;
    public override bool HasSideEffects { get; } = operand.HasSideEffects;
    internal ConstantFoldingError FoldingError => foldingResult.Error;

    public BoundUnaryOperator Operator { get; } = @operator;
    public BoundExpression Operand { get; } = operand;

    public override string ToString() => $"{Operator.SyntaxKind.GetText()} {Operand}";
}
