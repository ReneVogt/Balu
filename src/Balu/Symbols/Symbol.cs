namespace Balu.Symbols;
public abstract class Symbol
{
    public abstract SymbolKind Kind { get; }
    public string Name { get; }

    private protected Symbol(string name) => Name = name;

    public override string ToString() => $"{Name}";
}
