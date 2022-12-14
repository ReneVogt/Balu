namespace Balu.Symbols;

/// <summary>
/// Represents a function in Balu.
/// </summary>
public sealed class FunctionSymbol : Symbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Function;

    internal FunctionSymbol(string name) : base (name)
    {
    }
}
