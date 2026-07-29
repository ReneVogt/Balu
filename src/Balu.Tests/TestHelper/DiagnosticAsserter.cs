using System;
using System.Collections.Immutable;
using System.Linq;
using Balu.Diagnostics;
using Xunit;

namespace Balu.Tests.TestHelper;

static class DiagnosticAsserter
{
    internal static void AssertDiagnostics(AnnotatedText annotatedText, ImmutableArray<Diagnostic> actualDiagnostics, string? expectedDiagnostics = null, bool ignoreWarnings = true)
    {
        var expected = ParseExpectedDiagnostics(expectedDiagnostics);
        if (expected.Length != annotatedText.Spans.Length)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        var relevantDiagnostics = ignoreWarnings ? [.. actualDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)] : actualDiagnostics;
        var orderedActualDiagnostics = relevantDiagnostics.Where(d => !ignoreWarnings || d.Severity == DiagnosticSeverity.Error)
                                                          .OrderBy(diagnostic => diagnostic.Location.Text?.FileName)
                                                          .ThenBy(diagnostic => diagnostic.Location.Span.Start)
                                                          .ThenByDescending(diagnostic => diagnostic.Location.Span.Length)
                                                          .ToArray();
        var expectedTuples = expected.Zip(annotatedText.Spans, (diagnostic, span) => (span, diagnostic.id, diagnostic.message));
        var actualTuples = orderedActualDiagnostics.Select(diag => (span: diag.Location.Span, id: diag.IdString, message: diag.Message));
        Assert.Equal(expectedTuples, actualTuples);
    }
    internal static int AssertDiagnostics((string hintName, AnnotatedText annotated)[] inputs, ImmutableArray<Diagnostic> actualDiagnostics, string? expectedDiagnostics = null, bool ignoreWarnings = true)
    {
        var expected = ParseExpectedDiagnostics(expectedDiagnostics);
        var expectedSpanCount = inputs.Sum(x => x.annotated.Spans.Length);

        if (expected.Length != expectedSpanCount)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        if (ignoreWarnings)
            Assert.Equal(expectedSpanCount, actualDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        else
            Assert.Equal(expectedSpanCount, actualDiagnostics.Length);

        var expectedHintNames = inputs.SelectMany(input => Enumerable.Repeat(input.hintName, input.annotated.Spans.Length)).ToArray();
        var expectedSpans = inputs.SelectMany(input => input.annotated.Spans).ToArray();

        var orderedActualDiagnostics = actualDiagnostics.Where(d => !ignoreWarnings || d.Severity == DiagnosticSeverity.Error).OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length).ToArray();
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expectedHintNames[i], orderedActualDiagnostics[i].Location.FileName);
            Assert.Equal(expected[i].id, orderedActualDiagnostics[i].IdString);
            Assert.Equal(expected[i].message, orderedActualDiagnostics[i].Message);
            Assert.Equal(expectedSpans[i], orderedActualDiagnostics[i].Location.Span);
        }

        return expected.Length;
    }

    static (string id, string message)[] ParseExpectedDiagnostics(string? diagnostics) =>
        [.. AnnotatedText.UnindentLines(diagnostics).Select(ParseExpectedDiagnostic)];

    static (string id, string message) ParseExpectedDiagnostic(string diagnostic)
    {
        const int idLength = 6;
        const string separator = ": ";
        if (diagnostic.Length < idLength + separator.Length ||
            !diagnostic.StartsWith("BL", StringComparison.Ordinal) ||
            !diagnostic.Substring(2, 4).All(c => c is >= '0' and <= '9') ||
            !diagnostic.AsSpan(idLength).StartsWith(separator, StringComparison.Ordinal))
            throw new ArgumentException($"Expected diagnostic must have the format 'BLdddd: Message', but was '{diagnostic}'.");

        return (diagnostic[..idLength], diagnostic[(idLength + separator.Length)..]);
    }
}
