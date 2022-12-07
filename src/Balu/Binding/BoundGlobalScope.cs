using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }
    public BoundExpression Expression { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, BoundExpression expression, IEnumerable<VariableSymbol> variables, IEnumerable<Diagnostic> diagnostics)
    {
        Previous = previous;
        Expression = expression;
        Variables = variables.ToImmutableArray();
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
