using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundScope
{
    Dictionary<string, Symbol>? symbols;

    public BoundScope? Parent { get; }
    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryLookupSymbol(string name, out Symbol symbol)
    {
        symbol = null!;
        return (symbols?.TryGetValue(name, out symbol!) ?? false) || (Parent?.TryLookupSymbol(name, out symbol) ?? false);
    }
    public bool TryDeclareSymbol(Symbol symbol)
    {
        symbols ??= new();
        if (symbols.ContainsKey(symbol.Name)) return false;
        symbols.Add(symbol.Name, symbol);
        return true;
    }
    public ImmutableArray<Symbol> GetDeclaredSymbols() => symbols?.Values.ToImmutableArray() ?? ImmutableArray<Symbol>.Empty;
}
