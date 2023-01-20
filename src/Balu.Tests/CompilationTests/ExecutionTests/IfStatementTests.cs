using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("var a = 0 if true a = 1 a", 1)]
    [InlineData("var a = 0 if false a = 1 a", 0)]
    [InlineData("var a = 0 if true a = 1 else a = 2 a", 1)]
    [InlineData("var a = 0 if false a = 1 else a = 2 a", 2)]
    [InlineData("var a = 10 if a == 10 a = 5 a", 5)]
    [InlineData("var a = 10 if a != 10 a = 5 a", 10)]
    [InlineData("var a = 10 if a == 10 a = 5 else a = 20 a", 5)]
    [InlineData("var a = 10 if a != 10 a = 5 else a = 20 a", 20)]
    public void Execute_IfStatement_BasicallyWorks(string text, object? result) => text.AssertEvaluation(value: result);
    [Fact]
    public void Execute_IfStatement_Reports_WrongConditionType()
    {
        const string text = "if [(12 + 3)] {} else {}";
        const string diagnostics = @"
            Cannot convert 'int' to 'bool'.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Execute_ElseClause_Reports_UnexpectedToken()
    {
        const string text = @"
                    {
                        var x = 10
                        [else] x = 12
                    }
    ";
        const string diagnostics = @"
                Unexpected ElseKeyword ('else'), expected IdentifierToken.
    ";
        text.AssertEvaluation(diagnostics);
    }
}