namespace Balu.Symbols;
/// <summary>
/// Abstract base class for symbols in the Balu language.
/// </summary>
public abstract class Symbol
{
    /// <summary>
    /// The <see cref="Symbol"/> of this symbol.
    /// </summary>
    public abstract SymbolKind Kind { get; }

    /// <summary>
    /// The name of the symbol.
    /// </summary>
    public string Name { get; }

    private protected Symbol(string name) => Name = name;

    /// <inheritdoc/>
    public override string ToString() => $"{Name}";
}
