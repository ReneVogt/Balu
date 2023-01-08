using Balu.Symbols;

namespace Balu.Binding;

static class ConstantFolder
{
    public static BoundConstant? ComputeConstant(BoundExpression left, BoundBinaryOperator operation, BoundExpression right)
    {
        if (operation.OperatorKind == BoundBinaryOperatorKind.LogicalAnd)
        {
            if (left.Constant != null && !(bool)left.Constant.Value ||
                right.Constant != null && !(bool)right.Constant.Value)
                return new(false);
        }
        if (operation.OperatorKind == BoundBinaryOperatorKind.LogicalOr)
        {
            if (left.Constant != null && (bool)left.Constant.Value ||
                right.Constant != null && (bool)right.Constant.Value)
                return new(true);
        }

        return left.Constant is null || right.Constant is null
                   ? null
                   : operation.OperatorKind switch
                   {
                       BoundBinaryOperatorKind.Addition => operation.Type == TypeSymbol.String
                                                               ? new((string)left.Constant.Value + (string)right.Constant.Value)
                                                               : new((int)left.Constant.Value + (int)right.Constant.Value),
                       BoundBinaryOperatorKind.Substraction => new((int)left.Constant.Value - (int)right.Constant.Value),
                       BoundBinaryOperatorKind.Multiplication => new((int)left.Constant.Value * (int)right.Constant.Value),
                       BoundBinaryOperatorKind.Division => new((int)left.Constant.Value / (int)right.Constant.Value),
                       BoundBinaryOperatorKind.LogicalOr => new((bool)left.Constant.Value || (bool)right.Constant.Value),
                       BoundBinaryOperatorKind.LogicalAnd => new((bool)left.Constant.Value && (bool)right.Constant.Value),
                       BoundBinaryOperatorKind.BitwiseAnd => left.Type == TypeSymbol.Boolean
                                                                 ? new((bool)left.Constant.Value & (bool)right.Constant.Value)
                                                                 : new((int)left.Constant.Value & (int)right.Constant.Value),
                       BoundBinaryOperatorKind.BitwiseOr => left.Type == TypeSymbol.Boolean
                                                                ? new((bool)left.Constant.Value | (bool)right.Constant.Value)
                                                                : new((int)left.Constant.Value | (int)right.Constant.Value),
                       BoundBinaryOperatorKind.BitwiseXor => left.Type == TypeSymbol.Boolean
                                                                 ? new((bool)left.Constant.Value ^ (bool)right.Constant.Value)
                                                                 : new((int)left.Constant.Value ^ (int)right.Constant.Value),
                       BoundBinaryOperatorKind.Equals => new(Equals(left.Constant.Value, right.Constant.Value)),
                       BoundBinaryOperatorKind.NotEqual => new(!Equals(left.Constant.Value, right.Constant.Value)),
                       BoundBinaryOperatorKind.Less => new((int)left.Constant.Value < (int)right.Constant.Value),
                       BoundBinaryOperatorKind.LessOrEquals => new((int)left.Constant.Value <= (int)right.Constant.Value),
                       BoundBinaryOperatorKind.Greater => new((int)left.Constant.Value > (int)right.Constant.Value),
                       BoundBinaryOperatorKind.GreaterOrEquals => new((int)left.Constant.Value >= (int)right.Constant.Value),
                       _ => throw new BindingException($"Unknown binary operator '{operation.OperatorKind}'.")
                   };
    }
    public static BoundConstant? ComputeConstant(BoundUnaryOperator operation, BoundExpression operand) =>
        operand.Constant is null
            ? null
            : operation.OperatorKind switch
            {
                BoundUnaryOperatorKind.Identity => operand.Constant,
                BoundUnaryOperatorKind.Negation => new (-(int)operand.Constant.Value),
                BoundUnaryOperatorKind.LogicalNegation => new(!(bool)operand.Constant.Value),
                BoundUnaryOperatorKind.BitwiseNegation => new(~(int)operand.Constant.Value),
                _ => throw new BindingException($"Unknown unary operator '{operation.OperatorKind}'.")
            };
}