using System;
using System.Collections.Generic;
using System.Linq;

namespace Balu.Evaluation;

/// <summary>
/// Represents the result of a Balu code evaluation.
/// </summary>
public sealed class EvaluationResult
{
    /// <summary>
    /// The errors that occured during lexing, parsing or binding.
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; }
    /// <summary>
    /// The resulting value of the evalution. This is <c>null</c> if there are any <see cref="Diagnostics"/>.
    /// </summary>
    public object? Value { get; }

    internal EvaluationResult(object? value)
        : this(Array.Empty<Diagnostic>(), value) { }
    internal EvaluationResult(IEnumerable<Diagnostic> diagnostics, object? value) => (Diagnostics, Value) = (diagnostics.ToArray(), value);
}
