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
            IL000A: ldarg.2
            IL000B: call System.String System.String::Concat(System.String,System.String,System.String)
            IL0010: stloc.1
            IL0011: ldarg.0
            IL0012: ldarg.1
            IL0013: ldarg.2
            IL0014: ldarg.3
            IL0015: call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            IL001A: stloc.2
            IL001B: ldc.i4.5
            IL001C: newarr System.String
            IL0021: dup
            IL0022: ldc.i4.0
            IL0023: ldarg.0
            IL0024: stelem.ref
            IL0025: dup
            IL0026: ldc.i4.1
            IL0027: ldarg.1
            IL0028: stelem.ref
            IL0029: dup
            IL002A: ldc.i4.2
            IL002B: ldarg.2
            IL002C: stelem.ref
            IL002D: dup
            IL002E: ldc.i4.3
            IL002F: ldarg.3
            IL0030: stelem.ref
            IL0031: dup
            IL0032: ldc.i4.4
            IL0033: ldarg.s e
            IL0035: stelem.ref
            IL0036: call System.String System.String::Concat(System.String[])
            IL003B: stloc.3
            IL003C: ret
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
            IL000B: stloc.1
            IL000C: ldarg.0
            IL000D: ldstr hello
            IL0012: ldarg.1
            IL0013: ldstr worldhello
            IL0018: call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            IL001D: stloc.2
            IL001E: ret
";

        code.AssertIl("test", IL);

    }
}