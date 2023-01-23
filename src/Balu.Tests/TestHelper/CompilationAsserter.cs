using System.Collections.Generic;
using System.Linq;
using Balu.Diagnostics;
using Balu.Interpretation;
using Balu.Symbols;
using Xunit;
namespace Balu.Tests.TestHelper;

class CompilationAsserter
{
    public Interpreter Interpreter { get; } = new();

    internal void AssertScriptEvaluation(string code, string? expectedDiagnostics = null,
                                         IDictionary<GlobalVariableSymbol, object>? expectedGlobalVariables = null, object? value = null,
                                         bool ignoreWarnings = true)
    {
        var annotatedText = AnnotatedText.Parse(code);
        var actualDiagnostics = Interpreter.Execute(annotatedText.Text, ignoreWarnings);

        DiagnosticAsserter.AssertDiagnostics(annotatedText, actualDiagnostics, expectedDiagnostics, ignoreWarnings);
        if (actualDiagnostics.HasErrors() || !ignoreWarnings && actualDiagnostics.Any()) return;

        Assert.Equal(value, Interpreter.Result);
        if (expectedGlobalVariables is null) return;
        Assert.Equal(expectedGlobalVariables.Count, Interpreter.GlobalVariables.Count);
        Assert.Equal(expectedGlobalVariables.Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name),
                     Interpreter.GlobalVariables.Select(x => (x.Key.Name, x.Value)).OrderBy(x => x.Name));
    }
}