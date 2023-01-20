using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_Return_AllowedGloballyInScript()
    {
        "{ var i = 5 return i }".AssertEvaluation(value: 5);
    }
    [Fact]
    public void Execute_Return_ReportsUnexpectedExpression()
    {
        "function test() { return [[25]] }".AssertEvaluation(@"
            Cannot convert 'int' to 'void'.
            'test' does not have a return type and cannot return a value of type 'int'.");
    }
    [Fact]
    public void Execute_Return_ReportsMissingExpression()
    {
        @"
            function test() : int 
            { 
                [return] 
            }".AssertEvaluation("'test' needs to return a value of type 'int'.");
    }
    [Fact]
    public void Execute_Return_ReportsUnexpectedTokenIfEspressionIsMissing()
    {
        @"
            function test() : int 
            { 
                return [[}]]".AssertEvaluation(@"
                    Unexpected ClosedBraceToken ('}'), expected IdentifierToken.
                    'test' needs to return a value of type 'int', not '?'.
");
    }
    [Fact]
    public void Execute_Return_ReportsWrongExpressionType()
    {
        "function test() : int { return [[true]] }".AssertEvaluation(@"
                Cannot convert 'bool' to 'int'.
                'test' needs to return a value of type 'int', not 'bool'.");
    }
    [Fact]
    public void Execute_Return_ReportsNotAllPathsReturn()
    {
        "function test() : int { if false return 0 [}]".AssertEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Execute_Return_ReportsNotAllPathsReturnForEmptyFunction()
    {
        "function test() : int { [}]".AssertEvaluation("Not all code paths of function 'test' return a value of type 'int'.");
    }
    [Fact]
    public void Execute_Return_DetectsDeadPaths()
    {
        "function test() : int { if true return 47 } test()".AssertEvaluation(value: 47);
    }
}