using Balu.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Symbols;
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
    internal static void AssertEvaluation(this string code, string? diagnostics = null, IDictionary<GlobalVariableSymbol, object>? globalVariables = null, object? value = null)
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
    internal static void AssertLexerDiagnostics(this string code, string expected)
    {
        var annotatedText = AnnotatedText.Parse(code);
        SyntaxTree.ParseTokens(annotatedText.Text, out var actualDiagnostics);
        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expected);
    }
}
