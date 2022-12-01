using System;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;
sealed class BoundBinaryOperator
{
    static readonly Dictionary<(SyntaxKind syntaxKind, Type leftType, Type rightType), BoundBinaryOperator> operators = new()
    {
        [(SyntaxKind.PlusToken, typeof(int), typeof(int))] = new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.MinusToken, typeof(int), typeof(int))] = new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Substraction, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.StarToken, typeof(int), typeof(int))] = new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.SlashToken, typeof(int), typeof(int))] = new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.AmpersandAmpersandToken, typeof(bool), typeof(bool))] = new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.PipePipeToken, typeof(bool), typeof(bool))] = new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.EqualsEqualsToken, typeof(int), typeof(int))] = new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.Equals, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.BangEqualToken, typeof(int), typeof(int))] = new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.NotEqual, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.EqualsEqualsToken, typeof(bool), typeof(bool))] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.BangEqualToken, typeof(bool), typeof(bool))] = new(SyntaxKind.BangEqualToken, BoundBinaryOperatorKind.NotEqual, typeof(bool), typeof(bool), typeof(bool)),
    };

    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind OperatorKind { get; }
    public Type LeftType { get; }
    public Type RightType { get; }
    public Type Type { get; }

    BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind operatorKind, Type leftType, Type rightType, Type type)
    {
        SyntaxKind = syntaxKind;
        OperatorKind = operatorKind;
        LeftType = leftType;
        RightType = rightType;
        Type = type;
    }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType) =>
        operators.TryGetValue((syntaxKind, leftType, rightType), out var op) ? op : null;
}
