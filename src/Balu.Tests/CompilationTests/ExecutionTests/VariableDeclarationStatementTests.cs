using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_VariableDeclaration_Reports_Redeclaration()
    {
        const string text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
";
        const string diagnostics = @"
            Symbol 'x' is already declared.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Execute_VariableDeclaration_Reports_UnknownType()
    {
        const string text = "var x : [unknown] = 10";
        const string diagnostics = @"
            Undefined type 'unknown'.
";
        text.AssertEvaluation(diagnostics);
    }
    [Fact]
    public void Execute_VariableDeclaration_Reports_InvalidCast()
    {
        const string text = "var x : int [=] true";
        const string diagnostics = @"
            Cannot convert 'bool' to 'int'.
";
        text.AssertEvaluation(diagnostics);
    }
}