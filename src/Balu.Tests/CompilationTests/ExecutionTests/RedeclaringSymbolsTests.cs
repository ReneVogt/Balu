using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_RedeclaringSymbolsUsedInFunctions_FunctionsKeepWorkingOnOldSymbol()
    {
        var compilation = "function a():int { return 42 }".AssertScriptEvaluation();
        compilation = "function b() : int { return a() } b()".AssertScriptEvaluation(value: 42, previous: compilation);
        compilation = "var a = 23".AssertScriptEvaluation(previous: compilation);
        "b()".AssertScriptEvaluation(value: 42, previous: compilation);
        Assert.Fail("This does not work in REPL!");
    }
}