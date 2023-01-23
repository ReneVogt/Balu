using System.Collections.Generic;
using System.Linq;
using Balu.Diagnostics;
using Balu.Interpretation;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.TestHelper;

static class StaticCompilationAsserter{

    internal static void AssertScriptEvaluation(this string code, string? expectedDiagnostics = null, object? value = null, bool ignoreWarnings = true)
    {
        var annotatedText = AnnotatedText.Parse(code);
        var interpreter = new Interpreter();
        var actualDiagnostics = interpreter.Execute(annotatedText.Text, ignoreWarnings);

        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expectedDiagnostics, ignoreWarnings);
        if (actualDiagnostics.HasErrors() || !ignoreWarnings && actualDiagnostics.Any()) return;
            
        Assert.Equal(value, interpreter.Result);
    }
    internal static void AssertProgramDiagnostics(this IEnumerable<(string hintName, string code)> files, string? diagnostics = null)
    {
        var inputs = files.Select(x => (x.hintName, annotated: AnnotatedText.Parse(x.code))).OrderBy(x => x.hintName).ToArray();
        var compilation = Compilation.Create(inputs.Select(x => SyntaxTree.Parse(SourceText.From(x.annotated.Text, x.hintName))).ToArray());
        DiagnosticAsserter.AssertDiagnostics(inputs, compilation.Diagnostics, diagnostics);
    }
    internal static void AssertLexerDiagnostics(this string code, string expected)
    {
        var annotatedText = AnnotatedText.Parse(code);
        SyntaxTree.ParseTokens(annotatedText.Text, out var actualDiagnostics);
        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expected);
    }
}
