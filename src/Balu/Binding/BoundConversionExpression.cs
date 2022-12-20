using System.Collections.Generic;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundConversionExpression : BoundExpression
{
    public BoundExpression Expression { get; }
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    public override TypeSymbol Type { get; }
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Expression; }
    }

    public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
    {
        Type = type;
        Expression = expression;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new(Type, expression);
    }

    public override string ToString() => $"{Type}({Expression})";
}
