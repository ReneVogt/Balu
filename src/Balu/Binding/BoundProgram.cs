using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram(FunctionSymbol entryPoint, ImmutableArray<Symbol> symbols, ImmutableArray<Symbol> visibleSymbols, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
{
    public FunctionSymbol EntryPoint { get; } = entryPoint;
    public ImmutableArray<Symbol> Symbols { get; } = symbols;
    public ImmutableArray<Symbol> VisibleSymbols { get; } = visibleSymbols;
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; } = functions;
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}
