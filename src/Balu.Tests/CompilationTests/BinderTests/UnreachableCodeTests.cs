using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.BinderTests;

public class BinderTests
{
    [Theory]
    [InlineData("while true print(string(1)) [print(string(2))]")]
    [InlineData("while false [print(string(1))] print(string(2))")]
    [InlineData("function test() { return \r\n [print(\"unreachable\")]}")]
    [InlineData("var a = 0 if false [a = 1] a")]
    [InlineData("var a = 0 if true a = 1 else [a = 2] a")]
    [InlineData("var a = 0 if false [a = 1] else a = 2 a")]
    public void Lowerer_ReportsUnreachableCode(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "Unreachable code detected.", ignoreWarnings: false);
    }
    [Theory]
    [InlineData("while true { return 42 }", 42)]
    [InlineData("function test() { while true { return \r\n } }", null)]
    [InlineData("function test() : int { while true { return 12 } }", null)]
    public void Lowerer_InjectedReturnIsNotUnreachable(string code, object result)
    {
        code.AssertScriptEvaluation(value: result, ignoreWarnings: false);
    }
}