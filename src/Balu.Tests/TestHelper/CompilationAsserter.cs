using Balu.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Text;
using Xunit;
namespace Balu.Tests.TestHelper;

static class CompilationAsserter
{
    internal static (Compilation compilation, ImmutableDictionary<Symbol, object> globals) AssertScriptEvaluation(this string code, string? diagnostics = null, ImmutableDictionary<Symbol, object>? initializedGlobalVariables = null, IDictionary<GlobalVariableSymbol, object>? expectedGlobalVariables = null, object? value = null, Compilation? previous = null, bool ignoreWarnings = true)
    {
        var annotatedText = AnnotatedText.Parse(code);
        var compilation = Compilation.CreateScript(previous, SyntaxTree.Parse(annotatedText.Text));
        var result = compilation.Evaluate(ReferenceProvider.References, initializedGlobalVariables ?? ImmutableDictionary<Symbol, object>.Empty, ignoreWarnings);

        DiagnosticAsserter.AssertDiagnostics(annotatedText, result.Diagnostics, diagnostics, ignoreWarnings);
        if (result.Diagnostics.HasErrors() || !ignoreWarnings && result.Diagnostics.Any()) return (compilation, result.GlobalSymbols);
            
        Assert.Equal(value, result.Value);
        if (expectedGlobalVariables is null) return (compilation, result.GlobalSymbols);
        Assert.Equal(expectedGlobalVariables.Count, result.GlobalSymbols.Count(x => x.Key is GlobalVariableSymbol));
        Assert.Equal(expectedGlobalVariables.Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name),
                      result.GlobalSymbols.Where(x => x.Key is GlobalVariableSymbol).Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name));

        return (compilation, result.GlobalSymbols);
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
