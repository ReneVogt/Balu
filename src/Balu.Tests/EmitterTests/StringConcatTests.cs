using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_StringConcat_UsesCorrectOverload()
    {
        const string code = @"
            function test(a:string, b:string, c:string, d: string, e:string) {
                var x1 = a + b
                var x2 = a + b + c
                var x3 = a + b + c + d
                var x4 = a + b + c + d + e
            }
            return
";
        const string IL = @"
            IL0000: ldarg.0
            IL0001: ldarg.1
            IL0002: call System.String System.String::Concat(System.String,System.String)
            IL0007: stloc.0
            IL0008: ldarg.0
            IL0009: ldarg.1
            IL0010: ldarg.2
            IL0011: call System.String System.String::Concat(System.String,System.String,System.String)
            IL0016: stloc.1
            IL0017: ldarg.0
            IL0018: ldarg.1
            IL0019: ldarg.2
            IL0020: ldarg.3
            IL0021: call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            IL0026: stloc.2
            IL0027: ldc.i4.5
            IL0028: newarr System.String
            IL0033: dup
            IL0034: ldc.i4.0
            IL0035: ldarg.0
            IL0036: stelem.ref
            IL0037: dup
            IL0038: ldc.i4.1
            IL0039: ldarg.1
            IL0040: stelem.ref
            IL0041: dup
            IL0042: ldc.i4.2
            IL0043: ldarg.2
            IL0044: stelem.ref
            IL0045: dup
            IL0046: ldc.i4.3
            IL0047: ldarg.3
            IL0048: stelem.ref
            IL0049: dup
            IL0050: ldc.i4.4
            IL0051: ldarg.s e
            IL0053: stelem.ref
            IL0054: call System.String System.String::Concat(System.String[])
            IL0059: stloc.3
            IL0060: ret
";

        code.AssertIl("test", IL);
    }
    [Fact]
    public void Emitter_StringConcat_ConstantFolding()
    {
        const string code = @"
            function test(a:string, b:string) {
                let c1 = ""hello""
                let c2 = ""world""
                var x1 = a + c1 + b + c2 + c1
            }
            return
";
        const string IL = @"
            IL0000: ldstr hello
            IL0005: stloc.0
            IL0006: ldstr world
            IL0011: stloc.1
            IL0012: ldarg.0
            IL0013: ldstr hello
            IL0018: ldarg.1
            IL0019: ldstr worldhello
            IL0024: call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            IL0029: stloc.2
            IL0030: ret
";

        code.AssertIl("test", IL);

    }
}