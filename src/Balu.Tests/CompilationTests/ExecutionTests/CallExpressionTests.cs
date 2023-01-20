using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_Call_Reports_UndefinedFunction()
    {
        const string text = "{ [unknown]() }";
        const string diagnostics = @"
            Undefined function 'unknown'.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Execute_CallExpression_NoInfiniteLoopIfClosedParenthesisMissing()
    {
        const string text = "{print([[}]]";
        const string diagnostics = @"
            Unexpected ClosedBraceToken ('}'), expected IdentifierToken.
            Unexpected ClosedBraceToken ('}'), expected ClosedParenthesisToken.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Execute_CallExpression_InsideLoop()
    {
        "function test() {} var result = true while result { test() result = false } result ".AssertEvaluation(value: false);
    }
    [Fact]
    public void Execute_CallExpression_LocalVariables()
    {
        @"
              var sum = 0
                
              function inner(x:int)
              {
                var b = x
                sum = 100*sum + b
              }

              inner(42) inner(17) sum".AssertEvaluation(value: 4217);
    }
    [Fact]
    public void Execute_CallExpression_LocalVariablesNested()
    {
        @"
              var sum = 0 var sumx = 0
                
              function inner(x:int)
              {
                var b = x
                sum = 100*sum + b
              }
              function outer(x: int)
              {
                 var b = x
                 inner(x)
                 sumx = 100*sumx + 2*b
              }

              outer(42) outer(17) 10000*sum + sumx".AssertEvaluation(value: 42178434);
    }
    [Fact]
    public void Execute_CallExpression_Recursion()
    {
        @"
              var sum = 0
                
              function inner(x:int)
              {
                var b = x
                if x > 0 inner(x-1)
                sum = sum + b
              }

              inner(5) sum".AssertEvaluation(value: 15);
    }
    [Fact]
    public void Execute_CallExpression_RecursionNested()
    {
        @"
              var sum = 0 var sumb = 0
                
              function inner(x:int)
              {
                var b = x
                if x > 0 inner(x-1)
                sum = sum + b
              }
              function outer(x:int) { 
                var b = x
                inner(5)
                sumb = 100*sum + b                              
              }

              outer(42) sumb".AssertEvaluation(value: 1542);
    }

    [Fact]
    public void Execute_CallExpression_ReturnInsideLoop()
    {
        "function increase(i:int) : int { return i+1 } var result = 0 while result < 12 { result = increase(result) } result".AssertEvaluation(value: 12);
    }
}