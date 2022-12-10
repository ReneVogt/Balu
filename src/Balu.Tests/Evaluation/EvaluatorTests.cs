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
    [InlineData("12 < 3", false)]
    [InlineData("12 <= 3", false)]
    [InlineData("3 < 12", true)]
    [InlineData("3 <= 12", true)]
    [InlineData("12 > 3", true)]
    [InlineData("12 >= 3", true)]
    [InlineData("3 > 12", false)]
    [InlineData("3 >= 12", false)]
    [InlineData("5 < 5", false)]
    [InlineData("5 > 5", false)]
    [InlineData("5 <= 5", true)]
    [InlineData("5 >= 5", true)]
    public void Evaluate_Expression(string text, object expectedResult) => text.AssertEvaluation(value: expectedResult);

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
    [InlineData("1 [<] true", "Binary operator '<' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [<] 2", "Binary operator '<' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("false [<] true", "Binary operator '<' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [<=] true", "Binary operator '<=' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [<=] 2", "Binary operator '<=' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("false [<=] true", "Binary operator '<=' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [>] true", "Binary operator '>' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [>] 2", "Binary operator '>' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("false [>] true", "Binary operator '>' cannot be applied to types 'Boolean' and 'Boolean'.")]
    [InlineData("1 [>=] true", "Binary operator '>=' cannot be applied to types 'Int32' and 'Boolean'.")]
    [InlineData("false [>=] 2", "Binary operator '>=' cannot be applied to types 'Boolean' and 'Int32'.")]
    [InlineData("false [>=] true", "Binary operator '>=' cannot be applied to types 'Boolean' and 'Boolean'.")]
    public void Evaluate_BinaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Fact]
    public void Evaluate_Name_Reports_UndefinedName() => "var a = [bxy]".AssertEvaluation("Undefined name 'bxy'.");

    [Fact]
    public void Evaluate_Assignment_Reports_UndefinedName() => " [abc] = 12".AssertEvaluation("Undefined name 'abc'.");
    [Fact]
    public void Evaluate_Assignment_Reports_ReadOnly() =>
        "{ let abc = 12 [abc] = 10 }".AssertEvaluation("Variable 'abc' is readonly and cannot be assigned to.");
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

    [Theory]
    [InlineData("if true 1", 1)]
    [InlineData("if false 1", false)]
    [InlineData("if true 1 else 2", 1)]
    [InlineData("if false 1 else 2", 2)]
    [InlineData("{ var a = 10 if a == 10 a = 5 a }", 5)]
    [InlineData("{ var a = 10 if a != 10 a = 5 a }", 10)]
    [InlineData("{ var a = 10 if a == 10 a = 5 else a = 20 a }", 5)]
    [InlineData("{ var a = 10 if a != 10 a = 5 else a = 20 a }", 20)]
    public void Evaluate_IfStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_IfStatement_Reports_WrongConditionType()
    {
        const string text = "if [(12 + 3)] 1 else 0";
        const string diagnostics = @"
            Unexpected expression type 'Int32', expected 'Boolean'.
";
        text.AssertEvaluation(diagnostics);
    }
    //    [Fact]
    //    public void Evaluate_ElseClause_Reports_UnexpectedToken()
    //    {
    //        const string text = @"
    //                {
    //                    var x = 10
    //                    [else] x = 12
    //                }
    //";
    //        const string diagnostics = @"
    //            Variable 'x' is already declared.
    //";
    //        text.AssertEvaluation(diagnostics);
    //    }

    [Theory]
    [InlineData("{ var x = 0 while (x < 12) x = x + 1 x }", 12)]
    [InlineData("{ var result = 1 var i = 0 while (i < 5) { i = i + 1 result = result * 2} result }", 32)]
    [InlineData("{ var result = 0 var i = 0 while (i < 10) { i = i + 1 result = result + i} result }", 55)]
    public void Evaluate_WhileStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_WhileStatement_Reports_WrongConditionType()
    {
        const string text = "{var a=0 while [(12 + 3)] a = a + 1 }";
        const string diagnostics = @"
            Unexpected expression type 'Int32', expected 'Boolean'.
";
        text.AssertEvaluation(diagnostics);
    }

    [Theory]
    [InlineData("{ var result = 0 for i=0 to 10 result=result+i result }", 55)]
    public void Evaluate_ForStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Evaluate_ForStatement_Reports_WrongBoundaryTypes()
    {
        const string text = "{for i= [1>2] to [2>1] 12}";
        const string diagnostics = @"
            Unexpected expression type 'Boolean', expected 'Int32'.
            Unexpected expression type 'Boolean', expected 'Int32'.
";
        text.AssertEvaluation(diagnostics);
    }
}