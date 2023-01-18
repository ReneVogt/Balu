using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.AssignmentExpressions;

public class AssignmentExpressionTests
{
    [Theory]
    [InlineData("var a = 12 a = 7 a", 7)]
    [InlineData("var a = 12 a += 7 a", 19)]
    [InlineData("var a = 12 a -= 7 a", 5)]
    [InlineData("var a = 12 a *= 7 a", 84)]
    [InlineData("var a = 12 a /= 4 a", 3)]
    [InlineData("var a = 5 a &= 7 a", 5)]
    [InlineData("var a = 5 a |= 7 a", 7)]
    [InlineData("var a = true a &= false a", false)]
    [InlineData("var a = true a |= false a", true)]
    public void AssignmentExpressions_AssignsCorrectly(string code, object result) => code.AssertEvaluation(value: result);

    [Theory]
    [InlineData("var a = 12  a [+=] true", "Unary operator '!' cannot be applied to type 'int'.")]
    public void AssignmentExpressions_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

}