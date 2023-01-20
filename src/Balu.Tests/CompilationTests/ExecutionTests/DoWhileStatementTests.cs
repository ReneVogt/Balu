using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("var x = 0 do x = x + 1 while (x < 12) x", 12)]
    [InlineData("var result = 1 var i = 0 do { i = i + 1 result = result * 2} while (i < 5) result", 32)]
    [InlineData("var result = 0 var i = 0 do { i = i + 1 result = result + i} while (i < 10) result", 55)]
    public void Execute_DoWhileStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Execute_DoWhileStatement_Reports_WrongConditionType()
    {
        const string text = "{var a=0 do a = a + 1 while [(12 + 3)] }";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertEvaluation(diagnostics);
    }
}