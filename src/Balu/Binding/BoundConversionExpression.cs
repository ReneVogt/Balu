using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundConversionExpression : BoundExpression
{
    public BoundExpression Expression { get; }
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    public override TypeSymbol Type { get; }

    public BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression) : base(syntax)
    {
        Type = type;
        Expression = expression;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var expression = (BoundExpression)rewriter.Visit(Expression);
        return expression == Expression ? this : new(Syntax, Type, expression);
    }

    public override string ToString() => $"{Type}({Expression})";
}
