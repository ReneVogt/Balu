namespace Balu.Symbols;

/// <summary>
/// Represents a function parameter in Balu.
/// </summary>
public sealed class ParameterSymbol : LocalVariableSymbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(string name, TypeSymbol type) : base(name, false, type)
    {
    }
}
