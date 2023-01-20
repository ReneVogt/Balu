using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("var x = 0 while (x < 12) x = x + 1 x", 12)]
    [InlineData("var result = 1 var i = 0 while (i < 5) { i = i + 1 result = result * 2} result", 32)]
    [InlineData("var result = 0 var i = 0 while (i < 10) { i = i + 1 result = result + i} result", 55)]
    public void Script_WhileStatement_BasicallyWorks(string text, object? result) => text.AssertScriptEvaluation(value: result);
    [Fact]
    public void Script_WhileStatement_Reports_WrongConditionType()
    {
        const string text = "{var a=0 while [(12 + 3)] a = a + 1 }";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertScriptEvaluation(diagnostics);
    }
}