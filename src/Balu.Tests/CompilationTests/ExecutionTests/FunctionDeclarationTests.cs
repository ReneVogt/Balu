using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_FunctionDeclaration_ReportsMissingName()
    {
        "function [(]) : int { var i = 0 return i }".AssertScriptEvaluation(" Unexpected OpenParenthesisToken ('('), expected IdentifierToken.");
    }

}