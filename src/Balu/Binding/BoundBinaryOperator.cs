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
        [(SyntaxKind.AmpersandToken, typeof(int), typeof(int))] = new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.AmpersandToken, typeof(bool), typeof(bool))] = new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.AmpersandAmpersandToken, typeof(bool), typeof(bool))] = new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.PipeToken, typeof(int), typeof(int))] = new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.PipeToken, typeof(bool), typeof(bool))] = new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.PipePipeToken, typeof(bool), typeof(bool))] = new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.CircumflexToken, typeof(int), typeof(int))] = new(SyntaxKind.CircumflexToken, BoundBinaryOperatorKind.BitwiseXor, typeof(int), typeof(int), typeof(int)),
        [(SyntaxKind.CircumflexToken, typeof(bool), typeof(bool))] = new(SyntaxKind.CircumflexToken, BoundBinaryOperatorKind.BitwiseXor, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.EqualsEqualsToken, typeof(int), typeof(int))] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.BangEqualsToken, typeof(int), typeof(int))] = new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEqual, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.EqualsEqualsToken, typeof(bool), typeof(bool))] = new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.BangEqualsToken, typeof(bool), typeof(bool))] = new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEqual, typeof(bool), typeof(bool), typeof(bool)),
        [(SyntaxKind.LessToken, typeof(int), typeof(int))] = new(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int))] = new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.GreaterToken, typeof(int), typeof(int))] = new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, typeof(int), typeof(int), typeof(bool)),
        [(SyntaxKind.GreaterOrEqualsToken, typeof(int), typeof(int))] = new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, typeof(int), typeof(int), typeof(bool))
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
