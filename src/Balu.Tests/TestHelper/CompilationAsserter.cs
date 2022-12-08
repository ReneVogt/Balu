using System;
using Xunit;
namespace Balu.Tests.TestHelper;

static class CompilationAsserter
{
    internal static void AssertEvaluation(this string code, string? diagnostics = null, object? value = null)
    {
        var annotatedText = AnnotatedText.Parse(code);
        var result = Compilation.Evaluate(annotatedText.Text, new());

        var expectedDiagnostics = AnnotatedText.UnindentLines(diagnostics);
        if (expectedDiagnostics.Length != annotatedText.Spans.Length)
            throw new ArgumentException("The number of expected diagnostics must match the number of marked spans.");

        Assert.Equal(annotatedText.Spans.Length, result.Diagnostics.Length);

        for (int i = 0; i < expectedDiagnostics.Length; i++)
        {
            Assert.Equal(expectedDiagnostics[i], result.Diagnostics[i].Message);
            Assert.Equal(annotatedText.Spans[i], result.Diagnostics[i].Span);
        }

        if (expectedDiagnostics.Length == 0)
            Assert.Equal(value, result.Value);
    }
}
