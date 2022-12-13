using System.Collections.Generic;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;
sealed class BoundUnaryOperator
{
    static readonly Dictionary<(SyntaxKind syntaxKind, TypeSymbol operandType), BoundUnaryOperator> operators = new()
    {
        [(SyntaxKind.MinusToken, TypeSymbol.Integer)] =
            new (SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Integer),
        [(SyntaxKind.PlusToken, TypeSymbol.Integer)] =
            new (SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Integer),
        [(SyntaxKind.BangToken, TypeSymbol.Boolean)] =
            new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Boolean),
        [(SyntaxKind.TildeToken, TypeSymbol.Integer)] =
                new(SyntaxKind.TildeToken, BoundUnaryOperatorKind.BitwiseNegation, TypeSymbol.Integer)
    };

    public SyntaxKind SyntaxKind { get; }
    public BoundUnaryOperatorKind OperatorKind { get; }
    public TypeSymbol OperandType { get; }
    public TypeSymbol Type { get; }

    BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind operatorKind, TypeSymbol type) : this(syntaxKind, operatorKind, type, type){}
    BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind operatorKind, TypeSymbol operandType, TypeSymbol type)
    {
        SyntaxKind = syntaxKind;
        OperatorKind = operatorKind;
        OperandType = operandType;
        Type = type;
    }

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType) =>
        operators.TryGetValue((syntaxKind, operandType), out var op) ? op : null;
}
