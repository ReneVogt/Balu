using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("var result = 0 for i=0 to 10 result=result+i result", 55)]
    public void Script_ForStatement_BasicallyWorks(string text, object? result) => text.AssertScriptEvaluation(value: result);
    [Fact]
    public void Script_ForStatement_Reports_WrongBoundaryTypes()
    {
        const string text = "for i= [1>2] to [2>1] {}";
        const string diagnostics = @"
            Cannot convert 'bool' to 'int'.
            Cannot convert 'bool' to 'int'.
";
        text.AssertScriptEvaluation(diagnostics);
    }
}