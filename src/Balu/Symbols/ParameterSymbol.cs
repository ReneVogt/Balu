namespace Balu.Symbols;

public sealed class ParameterSymbol : LocalVariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;
    public int Ordinal { get; }

    internal ParameterSymbol(string name, TypeSymbol type, int ordinal) : base(name, false, type)
    {
        Ordinal = ordinal;
    }
}
