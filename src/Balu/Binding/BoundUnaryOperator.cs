using System;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;
sealed class BoundUnaryOperator
{
    static readonly Dictionary<(SyntaxKind syntaxKind, Type operandType), BoundUnaryOperator> operators = new()
    {
        [(SyntaxKind.MinusToken, typeof(int))] =
            new (SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int), typeof(int)),
        [(SyntaxKind.PlusToken, typeof(int))] =
            new (SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int), typeof(int)),
        [(SyntaxKind.BangToken, typeof(bool))] =
            new (SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool), typeof(bool)),
    };

    public SyntaxKind SyntaxKind { get; }
    public BoundUnaryOperatorKind OperatorKind { get; }
    public Type OperandType { get; }
    public Type Type { get; }

    BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind operatorKind, Type operandType, Type type)
    {
        SyntaxKind = syntaxKind;
        OperatorKind = operatorKind;
        OperandType = operandType;
        Type = type;
    }

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, Type operandType) =>
        operators.TryGetValue((syntaxKind, operandType), out var op) ? op : null;
}
