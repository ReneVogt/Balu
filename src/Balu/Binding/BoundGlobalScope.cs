using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public FunctionSymbol EntryPoint { get; }
    public BoundBlockStatement Statement { get; }
    public ImmutableArray<Symbol> ShadowedSymbols { get; }
    public ImmutableArray<Symbol> VisibleSymbols { get; }
    public ImmutableArray<Symbol> AllSymbols { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(FunctionSymbol entryPoint, BoundBlockStatement statement, ImmutableArray<Symbol> shadowedSymbols, ImmutableArray<Symbol> visibleSymbols, IEnumerable<Diagnostic> diagnostics)
    {
        EntryPoint = entryPoint;
        Statement = statement;
        ShadowedSymbols = shadowedSymbols;
        VisibleSymbols = visibleSymbols;
        AllSymbols = shadowedSymbols.AddRange(visibleSymbols);
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
