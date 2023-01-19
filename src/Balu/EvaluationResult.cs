using System.Collections.Immutable;
using Balu.Symbols;

namespace Balu;

public sealed class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Value { get; }
    public ImmutableDictionary<GlobalVariableSymbol, object> GlobalVariables { get; }

    internal EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value, ImmutableDictionary<GlobalVariableSymbol, object> globalVariables)
    {
        Diagnostics = diagnostics;
        Value = value;
        GlobalVariables = globalVariables;
    }
}
