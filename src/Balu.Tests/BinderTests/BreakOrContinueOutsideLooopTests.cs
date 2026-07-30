using TestHelpers;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [InlineData("[break]")]
    [InlineData(@"
        function test(a : int)
        {
            if (a > 0)
            {
                [break]
            }
        }")]
    public void Binder_ReportsBreakOutsideLoop(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1021: Invalid 'break' outside any loop.", ignoreWarnings: false);
    }
    [Theory]
    [InlineData("[continue]")]
    [InlineData(@"
        function test(a : int)
        {
            if (a > 0)
            {
                [continue]
            }
        }")]
    public void Binder_ReportsContinueOutsideLoop(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1021: Invalid 'continue' outside any loop.", ignoreWarnings: false);
    }
}
