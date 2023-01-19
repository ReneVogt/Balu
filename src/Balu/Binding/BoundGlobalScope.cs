using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public FunctionSymbol EntryPoint { get; }
    public BoundBlockStatement Statement { get; }
    public ImmutableArray<Symbol> Symbols { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(FunctionSymbol entryPoint, BoundBlockStatement statement, IEnumerable<Symbol> symbols, IEnumerable<Diagnostic> diagnostics)
    {
        EntryPoint = entryPoint;
        Statement = statement;
        Symbols = symbols.ToImmutableArray();
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
