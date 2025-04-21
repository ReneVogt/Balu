using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression) : BoundExpression(syntax)
{
    public override TypeSymbol Type { get; } = type;
    public BoundExpression Expression { get; } = expression;
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;

    public override string ToString() => $"{Type}({Expression})";
}
