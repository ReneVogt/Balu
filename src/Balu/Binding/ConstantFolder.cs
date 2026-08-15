using Balu.Symbols;

namespace Balu.Binding;

static class ConstantFolder
{
    public static ConstantFoldingResult Fold(BoundExpression left, BoundBinaryOperator operation, BoundExpression right)
    {
        if (operation.OperatorKind == BoundBinaryOperatorKind.LogicalAnd)
        {
            if (!right.HasSideEffects && left.Constant != null && !(bool)left.Constant.Value ||
                !left.HasSideEffects && right.Constant != null && !(bool)right.Constant.Value)
                return Constant(false);
        }
        if (operation.OperatorKind == BoundBinaryOperatorKind.LogicalOr)
        {
            if (!right.HasSideEffects && left.Constant != null && (bool)left.Constant.Value ||
                !left.HasSideEffects && right.Constant != null && (bool)right.Constant.Value)
                return Constant(true);
        }

        if (right.Constant is null) return default;
        if (operation.OperatorKind == BoundBinaryOperatorKind.Division)
        {
            var divisor = (int)right.Constant.Value;
            if (divisor == 0)
                return new(null, ConstantFoldingError.DivisionByZero);
            if (divisor == -1 && left.Constant is { Value: int.MinValue })
                return new(null, ConstantFoldingError.IntegerOverflow);
        }

        if (left.Constant is null) return default;

        return operation.OperatorKind switch
        {
            BoundBinaryOperatorKind.Addition => operation.Type == TypeSymbol.String
                                                    ? Constant((string)left.Constant.Value + (string)right.Constant.Value)
                                                    : Constant((int)left.Constant.Value + (int)right.Constant.Value),
            BoundBinaryOperatorKind.Substraction => Constant((int)left.Constant.Value - (int)right.Constant.Value),
            BoundBinaryOperatorKind.Multiplication => Constant((int)left.Constant.Value * (int)right.Constant.Value),
            BoundBinaryOperatorKind.Division => Constant((int)left.Constant.Value / (int)right.Constant.Value),
            BoundBinaryOperatorKind.LogicalOr => Constant((bool)left.Constant.Value || (bool)right.Constant.Value),
            BoundBinaryOperatorKind.LogicalAnd => Constant((bool)left.Constant.Value && (bool)right.Constant.Value),
            BoundBinaryOperatorKind.BitwiseAnd => left.Type == TypeSymbol.Boolean
                                                      ? Constant((bool)left.Constant.Value & (bool)right.Constant.Value)
                                                      : Constant((int)left.Constant.Value & (int)right.Constant.Value),
            BoundBinaryOperatorKind.BitwiseOr => left.Type == TypeSymbol.Boolean
                                                     ? Constant((bool)left.Constant.Value | (bool)right.Constant.Value)
                                                     : Constant((int)left.Constant.Value | (int)right.Constant.Value),
            BoundBinaryOperatorKind.BitwiseXor => left.Type == TypeSymbol.Boolean
                                                      ? Constant((bool)left.Constant.Value ^ (bool)right.Constant.Value)
                                                      : Constant((int)left.Constant.Value ^ (int)right.Constant.Value),
            BoundBinaryOperatorKind.Equals => Constant(Equals(left.Constant.Value, right.Constant.Value)),
            BoundBinaryOperatorKind.NotEqual => Constant(!Equals(left.Constant.Value, right.Constant.Value)),
            BoundBinaryOperatorKind.Less => Constant((int)left.Constant.Value < (int)right.Constant.Value),
            BoundBinaryOperatorKind.LessOrEquals => Constant((int)left.Constant.Value <= (int)right.Constant.Value),
            BoundBinaryOperatorKind.Greater => Constant((int)left.Constant.Value > (int)right.Constant.Value),
            BoundBinaryOperatorKind.GreaterOrEquals => Constant((int)left.Constant.Value >= (int)right.Constant.Value),
            _ => throw new BindingException($"Unknown binary operator '{operation.OperatorKind}'.")
        };
    }

    public static ConstantFoldingResult Fold(BoundUnaryOperator operation, BoundExpression operand) =>
        operand.Constant is null
            ? default
            : operation.OperatorKind switch
            {
                BoundUnaryOperatorKind.Identity => new(operand.Constant),
                BoundUnaryOperatorKind.Negation => Constant(-(int)operand.Constant.Value),
                BoundUnaryOperatorKind.LogicalNegation => Constant(!(bool)operand.Constant.Value),
                BoundUnaryOperatorKind.BitwiseNegation => Constant(~(int)operand.Constant.Value),
                _ => throw new BindingException($"Unknown unary operator '{operation.OperatorKind}'.")
            };

    static ConstantFoldingResult Constant(object value) => new(new BoundConstant(value));
}
