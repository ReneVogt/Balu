﻿using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_PostfixExpression_ReportsUndefinedName() => " [abc]++".AssertScriptEvaluation("Undefined variable 'abc'.");
    [Theory]
    [InlineData("{ let abc = 12 [abc]++ }", "Variable 'abc' is readonly and cannot be assigned to.")]
    [InlineData("{ var abc = true [abc--]", "Postfix operator '--' cannot be applied to type 'bool'.")]
    [InlineData("{ var abc = \"\" [abc++]", "Postfix operator '++' cannot be applied to type 'string'.")]
    [InlineData("function test(){} [test++]", "Postfix operator '++' cannot be applied to type 'string'.")]
    public void Script_PostfixExpression_ReportsDiagnostic(string code, string? diagnostics) => code.AssertScriptEvaluation(diagnostics);

    [Theory]
    [InlineData("var a = 12 var b = a++ a+b", 25)]
    [InlineData("var a = 12 a--", 12)]
    public void Script_PostfixExpression_CorrectResult(string code, object result) => code.AssertScriptEvaluation(value: result);
}