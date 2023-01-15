using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundConversionExpression : BoundExpression
{
    public override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;

    public BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression) : base(syntax)
    {
        Type = type;
        Expression = expression;
    }

    public override string ToString() => $"{Type}({Expression})";
}
