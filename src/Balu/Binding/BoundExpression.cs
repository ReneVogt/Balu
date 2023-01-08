using Balu.Symbols;
namespace Balu.Binding;

abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }

    public virtual BoundConstant? Constant => null;

    public override string ToString() => $"{Kind} ({Type})";
}