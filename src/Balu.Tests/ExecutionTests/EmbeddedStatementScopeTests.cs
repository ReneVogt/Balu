using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("function test(): int { if true var x = 1 var x = 2 return x } test()")]
    [InlineData("function test(): int { if true {} else var x = 1 var x = 2 return x } test()")]
    [InlineData("function test(): int { var run = true while run var x = run = false var x = 2 return x } test()")]
    [InlineData("function test(): int { do var x = 1 while false var x = 2 return x } test()")]
    [InlineData("function test(): int { for i = 1 to 1 var x = 1 var x = 2 return x } test()")]
    public void Script_EmbeddedStatementVariable_CanBeRedeclaredAfterBody(string code) =>
        code.AssertScriptEvaluation(value: 2);
}
