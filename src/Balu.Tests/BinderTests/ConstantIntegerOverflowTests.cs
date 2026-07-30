using TestHelpers;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [InlineData("[(-2147483647 - 1) / -1]")]
    [InlineData("let minValue = -2147483647 - 1 [minValue / -1]")]
    [InlineData("var value = 1 + [(-2147483647 - 1) / -1]")]
    [InlineData(@"
        function test()
        {
            var value = [(-2147483647 - 1) / -1]
        }")]
    public void Binder_ReportsConstantIntegerOverflow(string code)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: "BL1033: Constant expression causes an integer overflow.", ignoreWarnings: false);
    }

    [Fact]
    public void Binder_FoldsNonOverflowingIntegerDivisionAtBoundary()
    {
        const string code = "-2147483647 / -1";

        code.AssertScriptEvaluation(value: int.MaxValue);
    }

    [Theory]
    [InlineData("2147483647 + 1", int.MinValue)]
    [InlineData("-2147483647 - 2", int.MaxValue)]
    [InlineData("1073741824 * 2", int.MinValue)]
    [InlineData("-(-2147483647 - 1)", int.MinValue)]
    public void Binder_FoldsWrappingIntegerOverflow(string code, int expected)
    {
        code.AssertScriptEvaluation(value: expected);
    }
}
