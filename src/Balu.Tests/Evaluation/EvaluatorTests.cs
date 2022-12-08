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
    public void Evaluate_Expression(string text, object expectedResult)
    {
        var variables = new VariableDictionary();
        var result = Compilation.Evaluate(text, variables);
        Assert.Equal(expectedResult, result.Value);
    }

    [Theory]
    [InlineData("[!]1", "Unary operator '!' cannot be applied to type 'Int32'.")]
    [InlineData("[+]true", "Unary operator '+' cannot be applied to type 'Boolean'.")]
    [InlineData("[-]false", "Unary operator '-' cannot be applied to type 'Boolean'.")]
    public void Evaluate_UnaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Theory]
    [InlineData("1 [&&] true", "Binary operator '&&' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [&&] 2", "Binary operator '&&' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("1 [&&] 2", "Binary operator '&&' cannot be applied to types 'Int32' and 'Int32'.")]
    [InlineData("1 [||] true", "Binary operator '||' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [||] 2", "Binary operator '||' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("1 [||] 2", "Binary operator '||' cannot be applied to types 'Int32' and 'Int32'.")]
    [InlineData("1 [+] true", "Binary operator '+' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [+] 2", "Binary operator '+' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("true [+] false", "Binary operator '+' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [-] true", "Binary operator '-' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [-] 2", "Binary operator '-' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("true [-] false", "Binary operator '-' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [*] true", "Binary operator '*' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [*] 2", "Binary operator '*' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("true [*] false", "Binary operator '*' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [/] true", "Binary operator '/' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [/] 2", "Binary operator '/' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("true [/] false", "Binary operator '/' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [==] true", "Binary operator '==' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [==] 2", "Binary operator '==' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("1 [!=] true", "Binary operator '!=' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [!=] 2", "Binary operator '!=' cannot be applied to types 'Boolean' and 'Int32'.")]
    public void Evaluate_BinaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Fact]
    public void Evaluate_Name_Reports_UndefinedName() => "var a = [bxy]".AssertEvaluation("Undefined name 'bxy'.");

    [Fact]
    public void Evaluate_Assignment_Reports_UndefinedName() => " [abc] = 12".AssertEvaluation("Undefined name 'abc'.");
    [Fact]
    public void Evaluate_Assignment_Reports_ReadOnly() => "{ let abc = 12 [abc] = 10 }".AssertEvaluation("Variable 'abc' is readonly and cannot be assigned to.");
    [Theory]
    [InlineData("{ var abc = 12 abc [=] false }", "Cannot convert 'Boolean' to 'Int32'.")]
    [InlineData("{ var abc = true abc [=] 17 }", "Cannot convert 'Int32' to 'Boolean'.")]
    public void Evaluate_Assignment_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Fact]
    public void Evaluate_VariableDeclaration_Reports_Redeclaration()
    {
        const string text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
";
        const string diagnostics = @"
            Variable 'x' is already declared.
";
        text.AssertEvaluation(diagnostics);
    }
}
