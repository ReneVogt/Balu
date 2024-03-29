﻿using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_PrefixExpression_ReportsUndefinedName() => " ++[abc]".AssertScriptEvaluation("Undefined variable 'abc'.");
    [Theory]
    [InlineData("{ let abc = 12 ++[abc] }", "Variable 'abc' is readonly and cannot be assigned to.")]
    [InlineData("{ var abc = true [--abc] }", "Prefix operator '--' cannot be applied to type 'bool'.")]
    [InlineData("{ var abc = \"\" [++abc] }", "Prefix operator '++' cannot be applied to type 'string'.")]
    [InlineData("function test(){} --[test]", "Unexpected symbol kind 'Function', expected 'test' to be a variable or argument.")]
    public void Script_PrefixExpression_ReportsDiagnostic(string code, string? diagnostics) => code.AssertScriptEvaluation(diagnostics);

    [Theory]
    [InlineData("var a = 12 var b = ++a a+b", 26)]
    [InlineData("var a = 12 --a", 11)]
    [InlineData("var a = 12 var b = 2 var c = a + ++b c", 15)]
    [InlineData("var a = 12 var b = 2 var c = a + --b c", 13)]
    public void Script_PrefixExpression_CorrectResult(string code, object result) => code.AssertScriptEvaluation(value: result);
}