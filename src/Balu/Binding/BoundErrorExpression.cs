using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundErrorExpression : BoundExpression
{
    public override TypeSymbol Type => TypeSymbol.Error;
    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

    internal BoundErrorExpression(SyntaxNode syntax)
        : base(syntax)
    {
    }

    public override string ToString() => "?";
}
