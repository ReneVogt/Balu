using System.Collections.Generic;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Theory]
    [MemberData(nameof(ProvideShadowingSymbolsTests))]
    public void Binder_ReportsShadowingSymbols(string code, string diagnostics)
    {
        code.AssertScriptEvaluation(expectedDiagnostics: diagnostics, ignoreWarnings: false);
    }

    public static IEnumerable<object[]> ProvideShadowingSymbolsTests()
    {
        yield return new object[]
        {
            @"function test([a]:int){} var a = 0",
            "Parameter 'a' hides global variable 'a'."
        };
        yield return new object[]
        {
            @"var a = 0 { var [a] = 1} ",
            "Local variable 'a' hides global variable 'a'."
        };
        yield return new object[]
        {
            @"function test(){ var [a] = 12 } var a = 0",
            "Local variable 'a' hides global variable 'a'."
        };
        yield return new object[]
        {
            @"function test(a:int){var [a] = 0} ",
            "Local variable 'a' hides parameter 'a'."
        };
        yield return new object[]
        {
            @"function [println](a:int){}",
            "Function 'println' hides existing function 'println'."
        };
        yield return new object[]
        {
            @"function test(a:int){ var [println] = 12}",
            "Local variable 'println' hides existing function 'println'."
        };
        yield return new object[]
        {
            @"function test([println]:int){ }",
            "Parameter 'println' hides existing function 'println'."
        };
    }
}