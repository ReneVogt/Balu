using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("", null)]
    [InlineData("42", 42)]
    [InlineData("-42", -42)]
    [InlineData("+--+3", 3)]
    [InlineData("+---3", -3)]
    [InlineData("~0", -1)]
    [InlineData("~1", -2)]
    [InlineData("~-1", 0)]
    [InlineData("~42", -43)]
    [InlineData("2+3", 5)]
    [InlineData("2-3", -1)]
    [InlineData("2*3", 6)]
    [InlineData("12/3", 4)]
    [InlineData("12/3+2", 6)]
    [InlineData("12/(4+2)", 2)]
    [InlineData("12*3+2", 38)]
    [InlineData("12*(3-5)", -24)]
    [InlineData("1|0", 1)]
    [InlineData("1|2", 3)]
    [InlineData("1&0", 0)]
    [InlineData("1&1", 1)]
    [InlineData("2&3", 2)]
    [InlineData("0^0", 0)]
    [InlineData("2^2", 0)]
    [InlineData("2^3", 1)]
    [InlineData("1^0", 1)]
    [InlineData("1^1", 0)]
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
    [InlineData("false|false", false)]
    [InlineData("false|true", true)]
    [InlineData("true|false", true)]
    [InlineData("true|true", true)]
    [InlineData("false&false", false)]
    [InlineData("false&true", false)]
    [InlineData("true&false", false)]
    [InlineData("true&true", true)]
    [InlineData("false^false", false)]
    [InlineData("false^true", true)]
    [InlineData("true^false", true)]
    [InlineData("true^true", false)]
    [InlineData("3 == 3", true)]
    [InlineData("3 == 4", false)]
    [InlineData("(true || false) == (false || true)", true)]
    [InlineData("true != false", true)]
    [InlineData("2 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("var a = 12 (a = a * 12)", 144)]
    [InlineData("var a = 10 for i=0 to (a = a - 1) {} a", 9)]
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
    [InlineData("\"escapedChars: \\r\\n \\v\\t\"", "escapedChars: \r\n \v\t")]
    [InlineData("\"hello \" + \"world\"", "hello world")]
    [InlineData("\"hello\" + \" \" + \"world\"", "hello world")]
    [InlineData("\"test\" == \"test\"", true)]
    [InlineData("\"test\" != \"test\"", false)]
    [InlineData("\"test\" == \"diff\"", false)]
    [InlineData("\"test\" != \"diff\"", true)]
    public void Script_Expression_CorrectResults(string text, object expectedResult) => text.AssertScriptEvaluation(value: expectedResult);

    [Theory]
    [InlineData("{ [print] = 12 }", "Unexpected symbol kind 'Function', expected 'print' to be a variable or argument.")]
    [InlineData("{ var a = 7 [a](12) }", "Unexpected symbol kind 'GlobalVariable', expected 'a' to be a function.")]
    public void Script_Reports_SymbolTypeMisatch(string text, string diagnostics) => text.AssertScriptEvaluation(diagnostics);

    [Fact]
    public void Script_ConstantFolding_DoesNotRemoveSideEffects()
    {
        @"
            var result = false
            function SetResultAndReturnTrue() : bool
            {
                result = true
                return true
            }
            var test = SetResultAndReturnTrue() && false
            result
        ".AssertScriptEvaluation(value: true);
    }
}