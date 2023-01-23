using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.InterpreterTests;

public sealed class InterpreterTests
{
    [Fact]
    public void Interpreter_UsesOnlyErrorFreeCompilations()
    {
        var asserter = new CompilationAsserter();
        asserter.AssertScriptEvaluation("function a() { var x = [y] }", expectedDiagnostics: "Undefined name 'y'.");
        asserter.AssertScriptEvaluation("function c() : int { return 42 }");
        asserter.AssertScriptEvaluation("c()", value: 42);
    }

}