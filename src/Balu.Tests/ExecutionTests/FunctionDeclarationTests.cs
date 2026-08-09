using System.Collections.Generic;
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

    [Theory]
    [MemberData(nameof(ProvideBuiltInTypeFunctionDeclarations))]
    public void Script_FunctionDeclaration_ReportsBuiltInTypeName(string name, string code)
    {
        code.AssertScriptEvaluation($"BL1020: Function '{name}' is already declared.");
    }

    [Theory]
    [InlineData("int")]
    [InlineData("bool")]
    [InlineData("string")]
    [InlineData("any")]
    public void Program_FunctionDeclaration_ReportsBuiltInTypeName(string name)
    {
        new[] { ("test", $"function [{name}]() {{}} function main() {{}}") }
            .AssertProgramDiagnostics($"BL1020: Function '{name}' is already declared.");
    }

    public static IEnumerable<object[]> ProvideBuiltInTypeFunctionDeclarations()
    {
        foreach (var (name, argument) in new[]
                 {
                     ("int", "1"),
                     ("bool", "true"),
                     ("string", "\"value\""),
                     ("any", "1")
                 })
        {
            yield return new object[] { name, $"function [{name}]() {{}} {name}()" };
            yield return new object[] { name, $"function [{name}](value:any) {{}} {name}({argument})" };
            yield return new object[] { name, $"function [{name}](first:any, second:any) {{}} {name}(1, 2)" };
        }
    }

    [Theory]
    [InlineData("print")]
    [InlineData("println")]
    [InlineData("input")]
    [InlineData("random")]
    public void Script_FunctionDeclaration_ReportsBuiltInFunctionName(string name)
    {
        $"function [{name}]() {{}}".AssertScriptEvaluation($"BL1020: Function '{name}' is already declared.");
    }

    [Theory]
    [InlineData("print")]
    [InlineData("println")]
    [InlineData("input")]
    [InlineData("random")]
    public void Program_FunctionDeclaration_ReportsBuiltInFunctionName(string name)
    {
        new[] { ("test", $"function [{name}]() {{}} function main() {{}}") }
            .AssertProgramDiagnostics($"BL1020: Function '{name}' is already declared.");
    }

    [Fact]
    public void Script_VariablesAndParametersMayUseBuiltInTypeNames()
    {
        "function test(int:int):int { var bool = true var string = \"\" var any = 1 return int } test(42)"
            .AssertScriptEvaluation(value: 42);
    }
}
