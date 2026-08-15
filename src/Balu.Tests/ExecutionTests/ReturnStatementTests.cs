using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Text;
using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_Return_AllowedGloballyInScript()
    {
        "{ var i = 5 return i }".AssertScriptEvaluation(value: 5);
    }
    [Fact]
    public void Script_Return_ReportsUnexpectedExpression()
    {
        "function test() { return [[25]] }".AssertScriptEvaluation(@"
            BL1006: Cannot convert 'int' to 'void'.
            BL1024: 'test' does not have a return type and cannot return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_ReportsMissingExpression()
    {
        @"
            function test() : int 
            { 
                [return] 
            }".AssertScriptEvaluation("BL1023: 'test' needs to return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_ReportsUnexpectedTokenIfEspressionIsMissing()
    {
        var compilation = Compilation.CreateScript(null, SyntaxTree.Parse("function test() : int { return }"));

        Assert.Collection(
            compilation.Diagnostics,
            diagnostic =>
            {
                Assert.Equal(DiagnosticId.UnexpectedToken, diagnostic.Id);
                Assert.Equal(new TextSpan(31, 1), diagnostic.Location.Span);
            },
            diagnostic =>
            {
                Assert.Equal(DiagnosticId.ReturnTypeMismatch, diagnostic.Id);
                Assert.Equal(new TextSpan(31, 0), diagnostic.Location.Span);
            });
    }
    [Fact]
    public void Script_Return_ReportsWrongExpressionType()
    {
        "function test() : int { return [[true]] }".AssertScriptEvaluation(@"
                BL1006: Cannot convert 'bool' to 'int'.
                BL1024: 'test' needs to return a value of type 'int', not 'bool'.");
    }
    [Fact]
    public void Script_Return_ReportsNotAllPathsReturn()
    {
        "function test() : int { if false [return 0] [}]".AssertScriptEvaluation(@"
            BL1031: Unreachable code detected.
            BL1025: Not all code paths of function 'test' return a value of type 'int'.", ignoreWarnings: false);
    }
    [Fact]
    public void Script_Return_ReportsNotAllPathsReturnForEmptyFunction()
    {
        "function test() : int { [}]".AssertScriptEvaluation("BL1025: Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_DetectsDeadPaths()
    {
        "function test() : int { if true return 47 } test()".AssertScriptEvaluation(value: 47);
    }
}
