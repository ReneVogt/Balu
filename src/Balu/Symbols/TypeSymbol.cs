namespace Balu.Symbols;

/// <summary>
/// Represents a type in Balu.
/// </summary>
public sealed class TypeSymbol : Symbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Type;

    public static TypeSymbol Integer { get; } = new TypeSymbol("int");
    public static TypeSymbol Boolean { get; } = new TypeSymbol("bool");
    public static TypeSymbol String { get; } = new TypeSymbol("string");

    TypeSymbol(string name) : base (name)
    {
    }
}
