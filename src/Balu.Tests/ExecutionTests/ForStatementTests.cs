using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("var result = 0 for i=0 to 10 result=result+i result", 55)]
    public void Script_ForStatement_BasicallyWorks(string text, object? result) => text.AssertScriptEvaluation(value: result);
    [Fact]
    public void Script_ForStatement_DoesNotOverflowAtIntegerMaximum()
    {
        const string text = @"
            var count = 0
            for i = 2147483647 to 2147483647 {
                count++
                if count > 1
                    break
            }
            count
        ";

        text.AssertScriptEvaluation(value: 1);
    }
    [Fact]
    public void Script_ForStatement_Reports_WrongBoundaryTypes()
    {
        const string text = "for i= [1>2] to [2>1] {}";
        const string diagnostics = @"
            BL1006: Cannot convert 'bool' to 'int'.
            BL1006: Cannot convert 'bool' to 'int'.
";
        text.AssertScriptEvaluation(diagnostics);
    }
}
