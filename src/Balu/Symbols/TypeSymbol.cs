namespace Balu.Symbols;

/// <summary>
/// Represents a type in Balu.
/// </summary>
public sealed class TypeSymbol : Symbol
{
    /// <inheritdoc />
    public override SymbolKind Kind => SymbolKind.Type;

    /// <summary>
    /// The 'int' type.
    /// </summary>
    public static TypeSymbol Integer { get; } = new TypeSymbol("int");
    /// <summary>
    /// The 'bool' type.
    /// </summary>
    public static TypeSymbol Boolean { get; } = new TypeSymbol("bool");
    /// <summary>
    /// The 'string' type.
    /// </summary>
    public static TypeSymbol String { get; } = new TypeSymbol("string");
    /// <summary>
    /// A yet unknown type.
    /// </summary>
    public static TypeSymbol Error { get; } = new TypeSymbol("?");

    TypeSymbol(string name) : base (name)
    {
    }
}
