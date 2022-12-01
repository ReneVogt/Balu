using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

static class BindingFacts
{
    public static BoundUnaryOperatorKind UnaryOperatorKind(this SyntaxKind syntaxKind) => syntaxKind switch
    {
        SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
        SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
        SyntaxKind.BangToken => BoundUnaryOperatorKind.LogicalNegation,
        _ => throw new BindingException($"Cannot bind unary operator kind {syntaxKind}.")
    };
    public static BoundBinaryOperatorKind BinaryOperatorKind(this SyntaxKind syntaxKind) => syntaxKind switch
    {
        SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
        SyntaxKind.MinusToken => BoundBinaryOperatorKind.Substraction,
        SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
        SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
        SyntaxKind.AmpersandAmpersandToken => BoundBinaryOperatorKind.LogicalAnd,
        SyntaxKind.PipePipeToken => BoundBinaryOperatorKind.LogicalOr,
        _ => throw new BindingException($"Cannot bind binary operator kind {syntaxKind}.")
    };

    static readonly Dictionary<BoundUnaryOperatorKind, Type[]> unaryOperatorTypes = new()
    {
        [BoundUnaryOperatorKind.Identity] = new[] { typeof(int) },
        [BoundUnaryOperatorKind.Negation] = new[] { typeof(int) },
        [BoundUnaryOperatorKind.LogicalNegation] = new[] { typeof(bool) }
    };
    public static bool CanBeAppliedTo(this BoundUnaryOperatorKind operatorKind, Type expressionType) =>
        unaryOperatorTypes.TryGetValue(operatorKind, out var types) && types.Contains(expressionType);

    static readonly Dictionary<BoundBinaryOperatorKind, Type[]> binaryOperatorTypes = new()
    {
        [BoundBinaryOperatorKind.Addition] = new[] { typeof(int) },
        [BoundBinaryOperatorKind.Substraction] = new[] { typeof(int) },
        [BoundBinaryOperatorKind.Multiplication] = new[] { typeof(int) },
        [BoundBinaryOperatorKind.Division] = new[] { typeof(int) },
        [BoundBinaryOperatorKind.LogicalAnd] = new[] { typeof(bool) },
        [BoundBinaryOperatorKind.LogicalOr] = new[] { typeof(bool) }
    };
    public static bool CanBeAppliedTo(this BoundBinaryOperatorKind operatorKind, Type leftType, Type rightType) => binaryOperatorTypes.TryGetValue(operatorKind, out var types) && types.Contains(leftType) && types.Contains(rightType);
}
