using TestHelpers;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [InlineData("while true print(string(1)) [print(string(2))]")]
    [InlineData("while false [print(string(1))] print(string(2))")]
    [InlineData("function test() { return \r\n [print(\"unreachable\")]}")]
    [InlineData("var a = 0 if false [a = 1] a")]
    [InlineData("var a = 0 if true a = 1 else [a = 2] a")]
    [InlineData("var a = 0 if false [a = 1] else a = 2 a")]
    [InlineData("for i = 1 to 10 { continue [println(\"unreachable\") \r\n println(\"more unreachable\")] }")]
    [InlineData(@"
        function test()
        {
            if false
            {
                [println(""false"")]
            }
            println(""true"")
        }")]
    public void Lowerer_ReportsUnreachableCode(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1031: Unreachable code detected.", ignoreWarnings: false);
    }
    [Fact]
    public void Lowerer_ReportsUnreachableCodeInEachFile()
    {
        const string text1 = "while false [println(\"unreachable\")]";
        const string text2 = "while false [println(\"unreachable\")]";
        const string diagnostics = @"
            BL1031: Unreachable code detected.
            BL1031: Unreachable code detected.";

        new[] { ("file1.b", text1), ("file2.b", text2) }.AssertScriptDiagnostics(diagnostics, ignoreWarnings: false);
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
