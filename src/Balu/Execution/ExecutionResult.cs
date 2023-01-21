using System.Collections.Immutable;
using Balu.Diagnostics;
using Balu.Symbols;

namespace Balu.Execution;

public sealed class ExecutionResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Value { get; }
    public ImmutableDictionary<Symbol, object> GlobalSymbols { get; }

    internal ExecutionResult(ImmutableArray<Diagnostic> diagnostics, object? value, ImmutableDictionary<Symbol, object> globalSymbols)
    {
        Diagnostics = diagnostics;
        Value = value;
        GlobalSymbols = globalSymbols;
    }
}
