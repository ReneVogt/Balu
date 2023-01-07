#pragma warning disable CA1720
namespace Balu.Symbols;

public sealed class TypeSymbol : Symbol
{
    public static TypeSymbol Any { get; } = new TypeSymbol("any", true);
    public static TypeSymbol Void { get; } = new TypeSymbol("void", false);
    public static TypeSymbol Integer { get; } = new TypeSymbol("int", false);
    public static TypeSymbol Boolean { get; } = new TypeSymbol("bool", false);
    public static TypeSymbol String { get; } = new TypeSymbol("string", true);
    public static TypeSymbol Error { get; } = new TypeSymbol("?", false);

    public override SymbolKind Kind => SymbolKind.Type;
    public bool IsReferenceType { get; }

    TypeSymbol(string name, bool isReference) : base (name)
    {
        IsReferenceType = isReference;
    }
}
