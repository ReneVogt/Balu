using System.Collections.Generic;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;
sealed class BoundBinaryOperator
{
    static readonly Dictionary<(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType), BoundBinaryOperator> operators = new()
    {
        [(SyntaxKind.PlusToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Integer),
        [(SyntaxKind.PlusToken, TypeSymbol.String, TypeSymbol.String)] = new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),

        [(SyntaxKind.MinusToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Substraction, TypeSymbol.Integer),
        [(SyntaxKind.StarToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Integer),
        [(SyntaxKind.SlashToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Integer),
        [(SyntaxKind.AmpersandToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Integer),
        [(SyntaxKind.AmpersandToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Boolean),
        [(SyntaxKind.AmpersandAmpersandToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Boolean),
        [(SyntaxKind.PipeToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Integer),
        [(SyntaxKind.PipeToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Boolean),
        [(SyntaxKind.PipePipeToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Boolean),
        [(SyntaxKind.CircumflexToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.CircumflexToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Integer),
        [(SyntaxKind.CircumflexToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.CircumflexToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Boolean),
        [(SyntaxKind.EqualsEqualsToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Integer, TypeSymbol.Boolean),
        [(SyntaxKind.BangEqualsToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEqual, TypeSymbol.Integer, TypeSymbol.Boolean),
        [(SyntaxKind.EqualsEqualsToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Boolean),
        [(SyntaxKind.BangEqualsToken, TypeSymbol.Boolean, TypeSymbol.Boolean)] = new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEqual, TypeSymbol.Boolean),
        [(SyntaxKind.EqualsEqualsToken, TypeSymbol.String, TypeSymbol.String)] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.String, TypeSymbol.Boolean),
        [(SyntaxKind.BangEqualsToken, TypeSymbol.String, TypeSymbol.String)] = new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEqual, TypeSymbol.String, TypeSymbol.Boolean),
        [(SyntaxKind.LessToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, TypeSymbol.Integer, TypeSymbol.Boolean),
        [(SyntaxKind.LessOrEqualsToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, TypeSymbol.Integer, TypeSymbol.Boolean),
        [(SyntaxKind.GreaterToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, TypeSymbol.Integer, TypeSymbol.Boolean),
        [(SyntaxKind.GreaterOrEqualsToken, TypeSymbol.Integer, TypeSymbol.Integer)] = new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, TypeSymbol.Integer, TypeSymbol.Boolean)
    };

    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind OperatorKind { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }
    public TypeSymbol Type { get; }

    BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind operatorKind, TypeSymbol type) : this(syntaxKind, operatorKind, type, type, type) { }
    BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind operatorKind, TypeSymbol operandType, TypeSymbol operatorType)
        : this(syntaxKind, operatorKind, operandType, operandType, operatorType) { }
    BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind operatorKind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol type)
    {
        SyntaxKind = syntaxKind;
        OperatorKind = operatorKind;
        LeftType = leftType;
        RightType = rightType;
        Type = type;
    }

    public static BoundBinaryOperator BinaryPlus { get; } = Bind(SyntaxKind.PlusToken, TypeSymbol.Integer, TypeSymbol.Integer)!;
    public static BoundBinaryOperator LessOrEquals { get; } = Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Integer, TypeSymbol.Integer)!;

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType) =>
        operators.TryGetValue((syntaxKind, leftType, rightType), out var op) ? op : null;
    public object Apply(object? left, object? right)
    {
        if (LeftType == TypeSymbol.Integer && RightType == TypeSymbol.Integer)
            return Apply((int)left!, (int)right!);
        if (LeftType == TypeSymbol.Boolean && RightType == TypeSymbol.Boolean)
            return Apply((bool)left!, (bool)right!);
        if (LeftType == TypeSymbol.String && RightType == TypeSymbol.String)
            return Apply((string)left!, (string)right!);
        throw new BindingException($"Unexpected binary operand types '{LeftType}' and '{RightType}' for operator '{OperatorKind}' to target type '{Type}'.");
    }
    object Apply(int left, int right) => OperatorKind switch
    {
        BoundBinaryOperatorKind.Addition => left + right,
        BoundBinaryOperatorKind.BitwiseAnd => left & right,
        BoundBinaryOperatorKind.BitwiseOr => left | right,
        BoundBinaryOperatorKind.BitwiseXor => left ^ right,
        BoundBinaryOperatorKind.Division => left /right,
        BoundBinaryOperatorKind.Equals => left == right,
        BoundBinaryOperatorKind.Greater => left > right,
        BoundBinaryOperatorKind.GreaterOrEquals => left >= right,
        BoundBinaryOperatorKind.Less => left < right,
        BoundBinaryOperatorKind.LessOrEquals => left <= right,
        BoundBinaryOperatorKind.Multiplication => left * right,
        BoundBinaryOperatorKind.NotEqual => left != right,
        BoundBinaryOperatorKind.Substraction => left - right,
        _ => throw new BindingException($"Unexpected binary operator kind '{OperatorKind}' for operands of type '{LeftType}'.")
    };
    bool Apply(bool left, bool right) => OperatorKind switch
    {
        BoundBinaryOperatorKind.NotEqual => left != right,
        BoundBinaryOperatorKind.Equals => left == right,
        BoundBinaryOperatorKind.LogicalAnd => left && right,
        BoundBinaryOperatorKind.LogicalOr => left || right,
        BoundBinaryOperatorKind.BitwiseAnd => left & right,
        BoundBinaryOperatorKind.BitwiseOr => left | right,
        BoundBinaryOperatorKind.BitwiseXor => left ^ right,
        _ => throw new BindingException($"Unexpected binary operator kind '{OperatorKind}' for operands of type '{LeftType}'.")
    };
    object Apply(string left, string right) => OperatorKind switch
    {
        BoundBinaryOperatorKind.NotEqual => left != right,
        BoundBinaryOperatorKind.Equals => left == right,
        BoundBinaryOperatorKind.Addition => left + right,
        _ => throw new BindingException($"Unexpected binary operator kind '{OperatorKind}' for operands of type '{LeftType}'.")
    };
}
