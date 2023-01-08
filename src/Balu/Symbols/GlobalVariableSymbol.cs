using Balu.Binding;

namespace Balu.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, bool readOnly, TypeSymbol type, BoundConstant? constant)
        : base(name, readOnly, type, constant) { }
    public override SymbolKind Kind => SymbolKind.GlobalVariable;
    public override string ToString() => $"{Name} ({Type.Name}) (global)";
}