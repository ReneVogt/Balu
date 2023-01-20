using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_AssignmentExpression_ReportsUndefinedName() => " [abc] = 12".AssertEvaluation("Undefined variable 'abc'.");
    [Fact]
    public void Execute_AssignmentExpression_ReportsReadOnly() =>
        "{ let abc = 12 [abc] = 10 }".AssertEvaluation("Variable 'abc' is readonly and cannot be assigned to.");
    [Theory]
    [InlineData("{ var abc = 12 abc [=] false }", "Cannot convert 'bool' to 'int'.")]
    [InlineData("{ var abc = true abc [=] 17 }", "Cannot convert 'int' to 'bool'.")]
    [InlineData("var a = 12  a [+=] true", "Binary operator '+=' cannot be applied to types 'int' and 'bool'.")]
    [InlineData("var a = 12  a [+=] \"\"", "Binary operator '+=' cannot be applied to types 'int' and 'string'.")]
    [InlineData("var a = \"\"  a [+=] true", "Binary operator '+=' cannot be applied to types 'string' and 'bool'.")]
    [InlineData("var a = 12  a [^=] true", "Binary operator '^=' cannot be applied to types 'int' and 'bool'.")]
    public void Execute_AssignmentExpression_ReportsTypeMismatch(string code, string? diagnostics) => code.AssertEvaluation(diagnostics);

    [Theory]
    [InlineData("var a = 12 a = 7 a", 7)]
    [InlineData("var a = 12 a += 7 a", 19)]
    [InlineData("var a = 12 a -= 7 a", 5)]
    [InlineData("var a = 12 a *= 7 a", 84)]
    [InlineData("var a = 12 a /= 4 a", 3)]
    [InlineData("var a = 5 a &= 7 a", 5)]
    [InlineData("var a = 5 a |= 7 a", 7)]
    [InlineData("var a = true a &= false a", false)]
    [InlineData("var a = true a |= false a", true)]
    [InlineData("var a = true a ^= false a", true)]
    [InlineData("var a = true a ^= true a", false)]
    [InlineData("var a = 5 a ^= 7 a", 2)]
    [InlineData("var a = \"eins\" a += \"zwei\" a", "einszwei")]
    public void Execute_AssignmentExpression_AssignsCorrectly(string code, object result) => code.AssertEvaluation(value: result);
}