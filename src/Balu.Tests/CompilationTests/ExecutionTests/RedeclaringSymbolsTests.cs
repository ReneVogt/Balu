using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_RedeclaringSymbolsUsedInFunctions_FunctionsKeepWorkingOnOldSymbol()
    {
        var (compilation, globals) = "function a():int { return 42 }".AssertScriptEvaluation();
        (compilation, globals) = "function b() : int { return a() } b()".AssertScriptEvaluation(value: 42, previous: compilation, initializedGlobalVariables: globals);
        (compilation, globals) = "var a = 23".AssertScriptEvaluation(previous: compilation, initializedGlobalVariables: globals);
        "b()".AssertScriptEvaluation(value: 42, previous: compilation, initializedGlobalVariables: globals);
    }
}