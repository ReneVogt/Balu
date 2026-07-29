using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_RedeclaringSymbolsUsedInFunctions_FunctionsKeepWorkingOnOldSymbol()
    {
        using var asserter = new CompilationAsserter();
        asserter.AssertScriptEvaluation("function a():int { return 42 }");
        asserter.AssertScriptEvaluation("function b() : int { return a() } b()", value: 42);
        asserter.AssertScriptEvaluation("var a = 23");
        asserter.AssertScriptEvaluation("b()", value: 42);
    }
}
