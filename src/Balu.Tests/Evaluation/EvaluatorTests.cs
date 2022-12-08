using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.Evaluation;
public class EvaluatorTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("-42", -42)]
    [InlineData("+--+3", 3)]
    [InlineData("+---3", -3)]
    [InlineData("2+3", 5)]
    [InlineData("2-3", -1)]
    [InlineData("2*3", 6)]
    [InlineData("12/3", 4)]
    [InlineData("12/3+2", 6)]
    [InlineData("12/(4+2)", 2)]
    [InlineData("12*3+2", 38)]
    [InlineData("12*(3-5)", -24)]
    [InlineData("false", false)]
    [InlineData("true", true)]
    [InlineData("!false", true)]
    [InlineData("!true", false)]
    [InlineData("false && false", false)]
    [InlineData("true && false", false)]
    [InlineData("false && true", false)]
    [InlineData("true && true", true)]
    [InlineData("false || false", false)]
    [InlineData("true || false", true)]
    [InlineData("false || true", true)]
    [InlineData("true || true", true)]
    [InlineData("!true || true", true)]
    [InlineData("false || !false", true)]
    [InlineData("!(false && true)", true)]
    [InlineData("3 == 3", true)]
    [InlineData("3 == 4", false)]
    [InlineData("(true || false) == (false || true)", true)]
    [InlineData("true != false", true)]
    [InlineData("2 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("{var a = 12 (a = a * 12)}", 144)]
    public void Evaluate(string text, object expectedResult)
    {
        var variables = new VariableDictionary();
        var result = Compilation.Evaluate(text, variables);
        Assert.Equal(expectedResult, result.Value);
    }

    [Fact]
    public void Evaluate_VariableDeclaration_Reports_Redeclaration() =>
        @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
".AssertEvaluation("Variable 'x' is already declared.");
}
