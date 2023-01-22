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
        var expected = AnnotatedText.UnindentLines(expectedDiagnostics);
        if (expected.Length != annotatedText.Spans.Length)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        var relevantDiagnostics = ignoreWarnings ? actualDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToImmutableArray() : actualDiagnostics;
        var orderedActualDiagnostics = relevantDiagnostics.Where(d => !ignoreWarnings || d.Severity == DiagnosticSeverity.Error)
                                                          .OrderBy(diagnostic => diagnostic.Location.Text.FileName)
                                                          .ThenBy(diagnostic => diagnostic.Location.Span.Start)
                                                          .ThenByDescending(diagnostic => diagnostic.Location.Span.Length)
                                                          .ToArray();
        var expectedTuples = expected.Zip(annotatedText.Spans, (text, span) => (span, text));
        var actualTuples = orderedActualDiagnostics.Select(diag => (span: diag.Location.Span, text: diag.Message));
        Assert.Equal(expectedTuples, actualTuples);
    }
    internal static int AssertDiagnostics((string hintName, AnnotatedText annotated)[] inputs, ImmutableArray<Diagnostic> actualDiagnostics, string? expectedDiagnostics = null, bool ignoreWarnings = true)
    {
        var expectedLines = AnnotatedText.UnindentLines(expectedDiagnostics);
        var expectedSpanCount = inputs.Sum(x => x.annotated.Spans.Length);

        if (expectedLines.Length != expectedSpanCount)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        if (ignoreWarnings)
            Assert.Equal(expectedSpanCount, actualDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        else
            Assert.Equal(expectedSpanCount, actualDiagnostics.Length);

        var expectedHintNames = inputs.SelectMany(input => Enumerable.Repeat(input.hintName, input.annotated.Spans.Length)).ToArray();
        var expectedSpans = inputs.SelectMany(input => input.annotated.Spans).ToArray();

        var orderedActualDiagnostics = actualDiagnostics.Where(d => !ignoreWarnings || d.Severity == DiagnosticSeverity.Error).OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length).ToArray();
        for (int i = 0; i < expectedLines.Length; i++)
        {
            Assert.Equal(expectedHintNames[i], orderedActualDiagnostics[i].Location.FileName);
            Assert.Equal(expectedLines[i], orderedActualDiagnostics[i].Message);
            Assert.Equal(expectedSpans[i], orderedActualDiagnostics[i].Location.Span);
        }

        return expectedLines.Length;
    }
}