using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_GlobalStatement_ReportsInvalidExpressionStatement()
    {
        const string text = "42 * 17 function test() { [2*12] do {[17*12] }while false} do {[1+1]} while false";
        const string diagnostics = @"
            Only assignment or call expressions can be used as a statement.
            Only assignment or call expressions can be used as a statement.
            Only assignment or call expressions can be used as a statement.
";
        text.AssertEvaluation(diagnostics);
    }
}