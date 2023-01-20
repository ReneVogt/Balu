using Balu.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Symbols;
using Balu.Text;
using Xunit;
namespace Balu.Tests.TestHelper;

static class CompilationAsserter
{
    // HACK: find a way to get this for tests
    static readonly string[] referencedAssemblies = 
    {
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Runtime.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Runtime.Extensions.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0\System.Console.dll"
    };
    internal static void AssertScriptEvaluation(this string code, string? diagnostics = null, IDictionary<GlobalVariableSymbol, object>? globalVariables = null, object? value = null)
    {
        var annotatedText = AnnotatedText.Parse(code);
        var result = Compilation.CreateScript(null, SyntaxTree.Parse(annotatedText.Text)).Evaluate(referencedAssemblies, ImmutableDictionary<GlobalVariableSymbol, object>.Empty);

        var numberOfDiagnostics = DiagnosticAsserter.AssertDiagnostics(annotatedText, result.Diagnostics, diagnostics);
        if (numberOfDiagnostics > 0) return;
            
        Assert.Equal(value, result.Value);
        if (globalVariables is null) return;
        Assert.Equal(globalVariables.Count, result.GlobalVariables.Count);
        Assert.Equal(globalVariables.Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name),
                      result.GlobalVariables.Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name));
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
