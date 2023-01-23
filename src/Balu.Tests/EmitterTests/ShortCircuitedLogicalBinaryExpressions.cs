using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Theory]
    [InlineData("var called = false function test() : bool { called = true return true} var x = true && test() called", true)]
    [InlineData("var called = false function test() : bool { called = true return true} var x = false && test() called", false)]
    [InlineData("var called = false function test() : bool { called = true return true} var x = false || test() called", true)]
    [InlineData("var called = false function test() : bool { called = true return true} var x = true || test() called", false)]
    public void Emitter_LogicalBinaryExpression_ShortCircuited(string code, object? result) => code.AssertScriptEvaluation(value: result);
    [Fact]
    public void Emitter_LogicalAnd_ShortCircuited()
    {
        const string code = @"
            function test(a:bool, b:bool) : bool { return a && b }
            return
";
        const string IL = @"
            IL0000: ldarg.0
            IL0001: dup
            IL0002: brfalse.s IL_0006: ret
            IL0004: pop
            IL0005: ldarg.1
            IL0006: ret
";

        code.AssertIl("test", IL);

    }
    [Fact]
    public void Emitter_LogicalOr_ShortCircuited()
    {
        const string code = @"
            function test(a:bool, b:bool) : bool { return a || b } 
            return
";
        const string IL = @"
            IL0000: ldarg.0
            IL0001: dup
            IL0002: brtrue.s IL_0006: ret
            IL0004: pop
            IL0005: ldarg.1
            IL0006: ret
";

        code.AssertIl("test", IL);

    }
}