using System.Collections.Immutable;

namespace Balu.Evaluation;

public sealed class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Value { get; }

    internal EvaluationResult(object? value)
        : this(ImmutableArray<Diagnostic>.Empty, value) { }
    internal EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value) => (Diagnostics, Value) = (diagnostics, value);
}
