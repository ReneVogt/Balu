namespace Balu.Symbols;

public sealed class ParameterSymbol : LocalVariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(string name, TypeSymbol type) : base(name, false, type)
    {
    }
}
