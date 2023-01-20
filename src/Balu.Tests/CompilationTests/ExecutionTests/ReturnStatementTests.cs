using Balu.Tests.TestHelper;
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
            Cannot convert 'int' to 'void'.
            'test' does not have a return type and cannot return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_ReportsMissingExpression()
    {
        @"
            function test() : int 
            { 
                [return] 
            }".AssertScriptEvaluation("'test' needs to return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_ReportsUnexpectedTokenIfEspressionIsMissing()
    {
        @"
            function test() : int 
            { 
                return [[}]]".AssertScriptEvaluation(@"
                    Unexpected ClosedBraceToken ('}'), expected IdentifierToken.
                    'test' needs to return a value of type 'int', not '?'.
");
    }
    [Fact]
    public void Script_Return_ReportsWrongExpressionType()
    {
        "function test() : int { return [[true]] }".AssertScriptEvaluation(@"
                Cannot convert 'bool' to 'int'.
                'test' needs to return a value of type 'int', not 'bool'.");
    }
    [Fact]
    public void Script_Return_ReportsNotAllPathsReturn()
    {
        "function test() : int { if false return 0 [}]".AssertScriptEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_ReportsNotAllPathsReturnForEmptyFunction()
    {
        "function test() : int { [}]".AssertScriptEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Script_Return_DetectsDeadPaths()
    {
        "function test() : int { if true return 47 } test()".AssertScriptEvaluation(value: 47);
    }
}