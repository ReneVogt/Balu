using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram
{
    public BoundProgram? Previous { get; }
    public BoundGlobalScope GlobalScope { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram(BoundProgram?  previous, BoundGlobalScope globalScope, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
    {
        Previous = previous;
        GlobalScope = globalScope;
        Functions = functions;
        Diagnostics = diagnostics;
    }
}
