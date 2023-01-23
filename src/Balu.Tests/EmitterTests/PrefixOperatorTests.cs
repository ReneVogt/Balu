using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_PreIncrementOperatorExpression()
    {
        const string code = @"
            function test() {
                var a = 1
                var b = ++a
            }
            return
";
        const string IL = @"
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldloc.0
            IL0003: ldc.i4.1
            IL0004: add
            IL0005: dup
            IL0006: stloc.0
            IL0007: stloc.1
            IL0008: ret
";

        code.AssertIl("test", IL);
    }
    [Fact]
    public void Emitter_PreDecrementOperatorStatement()
    {
        const string code = @"
            function test() {
                var a = 1
                --a
            }
            return
";
        const string IL = @"
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldloc.0
            IL0003: ldc.i4.1
            IL0004: sub
            IL0005: dup
            IL0006: stloc.0
            IL0007: pop
            IL0008: ret
";

        code.AssertIl("test", IL);
    }

}