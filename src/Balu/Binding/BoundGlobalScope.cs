using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }
    public BoundStatement Statement { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<FunctionSymbol> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, BoundStatement statement, IEnumerable<VariableSymbol> variables, ImmutableArray<FunctionSymbol> functions, IEnumerable<Diagnostic> diagnostics)
    {
        Previous = previous;
        Statement = statement;
        Functions = functions;
        Variables = variables.ToImmutableArray();
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
