using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu;

public sealed class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Value { get; }
    public ImmutableDictionary<Symbol, object> GlobalSymbols{ get; }

    internal EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value, ImmutableDictionary<Symbol, object> globalSymbols)
    {
        Diagnostics = diagnostics;
        Value = value;
        GlobalSymbols = globalSymbols;
    }
}
