using System.Collections.Generic;
using System.Linq;
using Balu;
using Balu.Diagnostics;
using Balu.Interpretation;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace TestHelpers;

public static class StaticCompilationAsserter{

    public static void AssertScriptEvaluation(this string code, string? expectedDiagnostics = null, object? value = null, bool ignoreWarnings = true)
    {
        var annotatedText = AnnotatedText.Parse(code);
        using var interpreter = new Interpreter(ReferenceProvider.References);
        var actualDiagnostics = interpreter.Execute(annotatedText.Text, ignoreWarnings);

        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expectedDiagnostics, ignoreWarnings);
        if (actualDiagnostics.HasErrors() || !ignoreWarnings && actualDiagnostics.Any()) return;
            
        Assert.Equal(value, interpreter.Result);
    }
    public static void AssertProgramDiagnostics(this IEnumerable<(string hintName, string code)> files, string? diagnostics = null)
    {
        var inputs = files.Select(x => (x.hintName, annotated: AnnotatedText.Parse(x.code))).OrderBy(x => x.hintName).ToArray();
        var compilation = Compilation.Create([.. inputs.Select(x => SyntaxTree.Parse(SourceText.From(x.annotated.Text, x.hintName)))]);
        DiagnosticAsserter.AssertDiagnostics(inputs, compilation.Diagnostics, diagnostics);
    }
    public static void AssertScriptDiagnostics(this IEnumerable<(string hintName, string code)> files, string? diagnostics = null, bool ignoreWarnings = true)
    {
        var inputs = files.Select(x => (x.hintName, annotated: AnnotatedText.Parse(x.code))).OrderBy(x => x.hintName).ToArray();
        var compilation = Compilation.CreateScript(null, [.. inputs.Select(x => SyntaxTree.Parse(SourceText.From(x.annotated.Text, x.hintName)))]);
        DiagnosticAsserter.AssertDiagnostics(inputs, compilation.Diagnostics, diagnostics, ignoreWarnings);
    }
    public static void AssertLexerDiagnostics(this string code, string expected)
    {
        var annotatedText = AnnotatedText.Parse(code);
        SyntaxTree.ParseTokens(annotatedText.Text, out var actualDiagnostics);
        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expected);
    }
}
