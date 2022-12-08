using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }
    public BoundStatement Statement { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, BoundStatement statement, IEnumerable<VariableSymbol> variables, IEnumerable<Diagnostic> diagnostics)
    {
        Previous = previous;
        Statement = statement;
        Variables = variables.ToImmutableArray();
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
