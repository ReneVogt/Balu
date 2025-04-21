using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundGlobalScope(FunctionSymbol entryPoint, BoundBlockStatement statement, ImmutableArray<Symbol> shadowedSymbols, ImmutableArray<Symbol> visibleSymbols, IEnumerable<Diagnostic> diagnostics)
{
    public FunctionSymbol EntryPoint { get; } = entryPoint;
    public BoundBlockStatement Statement { get; } = statement;
    public ImmutableArray<Symbol> ShadowedSymbols { get; } = shadowedSymbols;
    public ImmutableArray<Symbol> VisibleSymbols { get; } = visibleSymbols;
    public ImmutableArray<Symbol> AllSymbols { get; } = shadowedSymbols.AddRange(visibleSymbols);
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics.ToImmutableArray();
}
