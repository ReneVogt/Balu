using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Theory]
    [InlineData("[!]1", "Unary operator '!' cannot be applied to type 'int'.")]
    [InlineData("[+]true", "Unary operator '+' cannot be applied to type 'bool'.")]
    [InlineData("[-]false", "Unary operator '-' cannot be applied to type 'bool'.")]
    [InlineData("[~]false", "Unary operator '~' cannot be applied to type 'bool'.")]
    public void Execute_UnaryOperator_Reports_TypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);
}