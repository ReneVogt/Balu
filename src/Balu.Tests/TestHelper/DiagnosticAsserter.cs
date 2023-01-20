using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Balu.Tests.TestHelper;

static class DiagnosticAsserter
{
    internal static int AssertDiagnostics(AnnotatedText annotatedText, ImmutableArray<Diagnostic> actualDiagnostics, string? expectedDiagnostics = null)
    {
        var expected = AnnotatedText.UnindentLines(expectedDiagnostics);
        if (expected.Length != annotatedText.Spans.Length)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        if (annotatedText.Spans.Length == 0)
            Assert.Empty(actualDiagnostics);

        Assert.Equal(annotatedText.Spans.Length, actualDiagnostics.Length);

        var orderedActualDiagnostics = actualDiagnostics.OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length).ToArray();
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], orderedActualDiagnostics[i].Message);
            Assert.Equal(annotatedText.Spans[i], orderedActualDiagnostics[i].Location.Span);
        }

        return expected.Length;
    }
    internal static int AssertDiagnostics((string hintName, AnnotatedText annotated)[] inputs, ImmutableArray<Diagnostic> actualDiagnostics, string? expectedDiagnostics = null)
    {
        var expectedLines = AnnotatedText.UnindentLines(expectedDiagnostics);
        var expectedSpanCount = inputs.Sum(x => x.annotated.Spans.Length);

        if (expectedLines.Length != expectedSpanCount)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        if (expectedSpanCount == 0)
            Assert.Empty(actualDiagnostics);

        Assert.Equal(expectedSpanCount, actualDiagnostics.Length);

        var expectedHintNames = inputs.SelectMany(input => Enumerable.Repeat(input.hintName, input.annotated.Spans.Length)).ToArray();
        var expectedSpans = inputs.SelectMany(input => input.annotated.Spans).ToArray();

        var orderedActualDiagnostics = actualDiagnostics.OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length).ToArray();
        for (int i = 0; i < expectedLines.Length; i++)
        {
            Assert.Equal(expectedHintNames[i], orderedActualDiagnostics[i].Location.FileName);
            Assert.Equal(expectedLines[i], orderedActualDiagnostics[i].Message);
            Assert.Equal(expectedSpans[i], orderedActualDiagnostics[i].Location.Span);
        }

        return expectedLines.Length;
    }
}