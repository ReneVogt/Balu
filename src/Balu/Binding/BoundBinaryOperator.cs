﻿using System;
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
    };

    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind OperatorKind { get; }
    public Type LeftType { get; }
    public Type RightType { get; }
    public Type ResultType { get; }

    BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind operatorKind, Type leftType, Type rightType, Type resultType)
    {
        SyntaxKind = syntaxKind;
        OperatorKind = operatorKind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType) =>
        operators.TryGetValue((syntaxKind, leftType, rightType), out var op) ? op : null;
}