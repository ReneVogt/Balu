#pragma warning disable CA1720
namespace Balu.Symbols;

public sealed class TypeSymbol : Symbol
{
    public static TypeSymbol Any { get; } = new TypeSymbol("any");
    public static TypeSymbol Void { get; } = new TypeSymbol("void");
    public static TypeSymbol Integer { get; } = new TypeSymbol("int");
    public static TypeSymbol Boolean { get; } = new TypeSymbol("bool");
    public static TypeSymbol String { get; } = new TypeSymbol("string");
    public static TypeSymbol Error { get; } = new TypeSymbol("?");

    public override SymbolKind Kind => SymbolKind.Type;

    TypeSymbol(string name) : base (name)
    {
    }
}
