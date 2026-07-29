using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_FunctionDeclaration_ReportsMissingName()
    {
        "function [(]) : int { var i = 0 return i }".AssertScriptEvaluation(" BL0001: Unexpected OpenParenthesisToken ('('), expected IdentifierToken.");
    }

}
