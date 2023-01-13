using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }

    public virtual BoundConstant? Constant => null;
    public virtual bool HasSideEffects => false;

    private protected BoundExpression(SyntaxNode syntax) : base(syntax){}

    public override string ToString() => $"{Kind} ({Type})";
}