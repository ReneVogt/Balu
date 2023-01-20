using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_FunctionDeclaration_ReportsMissingName()
    {
        "function [(]) : int { var i = 0 return i }".AssertEvaluation(" Unexpected OpenParenthesisToken ('('), expected IdentifierToken.");
    }

}