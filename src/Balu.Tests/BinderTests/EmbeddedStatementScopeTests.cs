using TestHelpers;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [InlineData("function test() { if true var x = 1 var y = [x] }")]
    [InlineData("function test() { if true {} else var x = 1 var y = [x] }")]
    [InlineData("function test() { while false var x = 1 var y = [x] }")]
    [InlineData("function test() { do var x = 1 while false var y = [x] }")]
    [InlineData("function test() { for i = 1 to 1 var x = 1 var y = [x] }")]
    public void Binder_EmbeddedStatementVariable_IsNotVisibleAfterBody(string code) =>
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1005: Undefined name 'x'.");

    [Fact]
    public void Binder_DoWhileBodyVariable_IsNotVisibleInCondition() =>
        "function test() { do var x = 1 while [x] == 1 }".AssertScriptEvaluation(
            expectedDiagnostics: "BL1005: Undefined name 'x'.");

    [Fact]
    public void Binder_IfBranches_HaveIndependentScopes() =>
        "function test(value: bool) { if value var x = 1 else var x = 2 } test(true)".AssertScriptEvaluation();
}
