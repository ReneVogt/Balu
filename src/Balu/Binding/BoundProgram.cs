using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;

sealed class BoundProgram
{
    public BoundGlobalScope GlobalScope { get; }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram(BoundGlobalScope globalScope, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, ImmutableArray<Diagnostic> diagnostics)
    {
        GlobalScope = globalScope;
        Functions = functions;
        Diagnostics = diagnostics;
    }
}
