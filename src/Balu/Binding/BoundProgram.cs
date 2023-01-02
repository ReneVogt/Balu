using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram
{
    public BoundProgram? Previous { get; }
    public FunctionSymbol EntryPoint { get; }
    public ImmutableArray<Symbol> Symbols { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram(BoundProgram?  previous, FunctionSymbol entryPoint, ImmutableArray<Symbol> symbols, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
    {
        Previous = previous;
        EntryPoint = entryPoint;
        Symbols = symbols;
        Functions = functions;
        Diagnostics = diagnostics;
    }
}
