namespace Balu.Symbols;

public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, bool readOnly, TypeSymbol type)
        : base(name, readOnly, type) { }
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.LocalVariable;
    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Type.Name}) (local)";
}