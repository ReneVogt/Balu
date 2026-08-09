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

    [Fact]
    public void Script_RedeclaringFunctionFromPreviousSubmission_ReportsWarningAndUsesNewFunction()
    {
        using var asserter = new CompilationAsserter();
        asserter.AssertScriptEvaluation("function value():int { return 1 }");
        asserter.AssertScriptEvaluation(
            "function [value]():int { return 2 }",
            "BL1009: Function 'value' hides existing function 'value'.",
            ignoreWarnings: false);
        asserter.AssertScriptEvaluation("function value():int { return 2 }");
        asserter.AssertScriptEvaluation("value()", value: 2);
    }

    [Fact]
    public void Script_DeclaringBuiltInFunctionAfterHidingItWithVariable_ReportsError()
    {
        using var asserter = new CompilationAsserter();
        asserter.AssertScriptEvaluation("var print = 1");
        asserter.AssertScriptEvaluation(
            "function [print]() {}",
            "BL1020: Function 'print' is already declared.");
    }
}
