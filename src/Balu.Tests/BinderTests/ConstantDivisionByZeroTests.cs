using TestHelpers;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [InlineData("[1/0]")]
    [InlineData("[(12 + 1) / (3 - 3)]")]
    [InlineData("var y = 7 + [3/0]")]
    [InlineData("let zero = 2 - 2 [10 / zero]")]
    [InlineData(@"
        function test()
        {
            var x = [1 / 0]
        }")]
    public void Binder_ReportsConstantDivisionByZero(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1032: Constant division by zero.", ignoreWarnings: false);
    }
}
