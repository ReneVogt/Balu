using Balu.Syntax;
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

        if (annotatedText.Spans.Length == 0)
            Assert.Empty(result.Diagnostics);

        Assert.Equal(annotatedText.Spans.Length, result.Diagnostics.Length);

        for (int i = 0; i < expectedDiagnostics.Length; i++)
        {
            Assert.Equal(expectedDiagnostics[i], result.Diagnostics[i].Message);
            Assert.Equal(annotatedText.Spans[i], result.Diagnostics[i].Span);
        }

        if (expectedDiagnostics.Length == 0)
            Assert.Equal(value, result.Value);
    }
    internal static void AssertLexerDiagnostics(this string code, string expected)
    {
        var annotatedText = AnnotatedText.Parse(code);
        SyntaxTree.ParseTokens(annotatedText.Text, out var actualDiagnostics);

        var expectedDiagnostics = AnnotatedText.UnindentLines(expected);
        if (expectedDiagnostics.Length != annotatedText.Spans.Length)
            throw new ArgumentException("The number of expected expectedDiagnostics must match the number of marked spans.");
        if (expectedDiagnostics.Length == 0)
            throw new ArgumentException("At least one diagnostic should be expected by this asserter.");

        Assert.Equal(annotatedText.Spans.Length, actualDiagnostics.Length);

        for (int i = 0; i < expectedDiagnostics.Length; i++)
        {
            Assert.Equal(expectedDiagnostics[i], actualDiagnostics[i].Message);
            Assert.Equal(annotatedText.Spans[i], actualDiagnostics[i].Span);
        }
    }
}
