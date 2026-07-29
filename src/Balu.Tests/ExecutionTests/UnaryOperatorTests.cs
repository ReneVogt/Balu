using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("[!]1", "BL1001: Unary operator '!' cannot be applied to type 'int'.")]
    [InlineData("[+]true", "BL1001: Unary operator '+' cannot be applied to type 'bool'.")]
    [InlineData("[-]false", "BL1001: Unary operator '-' cannot be applied to type 'bool'.")]
    [InlineData("[~]false", "BL1001: Unary operator '~' cannot be applied to type 'bool'.")]
    public void Script_UnaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertScriptEvaluation(diagnostics);
}
