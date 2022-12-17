namespace Balu.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, bool readOnly, TypeSymbol type)
        : base(name, readOnly, type) { }
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.GlobalVariable;
    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Type.Name}) (global)";
}