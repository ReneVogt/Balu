using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.EmitterTests;

public class EmitterTests
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
            ldarg.0
            ldarg.1
            call System.String System.String::Concat(System.String,System.String)
            stloc.0
            ldarg.0
            ldarg.1
            ldarg.2
            call System.String System.String::Concat(System.String,System.String,System.String)
            stloc.1
            ldarg.0
            ldarg.1
            ldarg.2
            ldarg.3
            call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            stloc.2
            ldc.i4.5
            newarr System.String
            dup
            ldc.i4.0
            ldarg.0
            stelem.ref
            dup
            ldc.i4.1
            ldarg.1
            stelem.ref
            dup
            ldc.i4.2
            ldarg.2
            stelem.ref
            dup
            ldc.i4.3
            ldarg.3
            stelem.ref
            dup
            ldc.i4.4
            ldarg.s e
            stelem.ref
            call System.String System.String::Concat(System.String[])
            stloc.3
            ret
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
            ldstr hello
            stloc.0
            ldstr world
            stloc.1
            ldarg.0
            ldstr hello
            ldarg.1
            ldstr worldhello
            call System.String System.String::Concat(System.String,System.String,System.String,System.String)
            stloc.2
            ret
";

        code.AssertIl("test", IL);

    }
}