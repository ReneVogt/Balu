using System;
using Balu.Syntax;

namespace Balu.Binding;

static class BindingFacts
{
    public static BoundUnaryOperatorKind UnaryOperatorKind(this SyntaxKind syntaxKind) => syntaxKind switch
    {
        SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
        SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
        _ => throw new BindingException($"Cannot bind unary operator kind {syntaxKind}.")
    };
    public static BoundBinaryOperatorKind BinaryOperatorKind(this SyntaxKind syntaxKind) => syntaxKind switch
    {
        SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
        SyntaxKind.MinusToken => BoundBinaryOperatorKind.Substraction,
        SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
        SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
        _ => throw new BindingException($"Cannot bind binary operator kind {syntaxKind}.")
    };
    public static bool CanBeAppliedTo(this BoundUnaryOperatorKind operatorKind, Type expressionType) => Enum.IsDefined(typeof(BoundUnaryOperatorKind), operatorKind) && expressionType == typeof(int);
    public static bool CanBeAppliedTo(this BoundBinaryOperatorKind operatorKind, Type leftType, Type rightType) => Enum.IsDefined(typeof(BoundBinaryOperatorKind), operatorKind) && leftType == typeof(int) && rightType == typeof(int);
}
