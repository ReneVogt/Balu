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
            Assert.Equal(expected[i], actualDiagnostics[i].Message);
            Assert.Equal(annotatedText.Spans[i], orderedActualDiagnostics[i].Location.Span);
        }

        return expected.Length;
    }
}