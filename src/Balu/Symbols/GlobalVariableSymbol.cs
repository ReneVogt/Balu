namespace Balu.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, bool readOnly, TypeSymbol type)
        : base(name, readOnly, type) { }
    public override SymbolKind Kind => SymbolKind.GlobalVariable;
    public override string ToString() => $"{Name} ({Type.Name}) (global)";
}