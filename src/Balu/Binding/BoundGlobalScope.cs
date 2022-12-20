﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu.Binding;
sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }
    public BoundBlockStatement Statement { get; }
    public ImmutableArray<Symbol> Symbols { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, BoundBlockStatement statement, IEnumerable<Symbol> symbols, IEnumerable<Diagnostic> diagnostics)
    {
        Previous = previous;
        Statement = statement;
        Symbols = symbols.ToImmutableArray();
        Diagnostics = diagnostics.ToImmutableArray();
    }

}
