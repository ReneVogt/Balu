using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_DoWhile_BlockBodyDebug()
    {
        const string code = @"
            function test(i:int) [{]
                [var i = 0]
                do
                [{]
                    [if i > 3]
                        [break]
                    [if i > 5]
                        [continue]
                    [i = 1]
                [}] [while true]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: nop
            IL0004: ldloc.0
            IL0005: ldc.i4.3
            IL0006: cgt
            IL0008: brfalse.s IL_000c: ldloc.0
            IL000A: br.s IL_001b: br.s IL_001d
            IL000C: ldloc.0
            IL000D: ldc.i4.5
            IL000E: cgt
            IL0010: brfalse.s IL_0014: ldc.i4.1
            IL0012: br.s IL_0019: br.s IL_0003
            IL0014: ldc.i4.1
            IL0015: dup
            IL0016: stloc.0
            IL0017: pop
            IL0018: nop
            IL0019: br.s IL_0003: nop
            IL001B: br.s IL_001d: ret
            IL001D: ret
";
        code.AssertIlAndSymbols("test", il);
    }
    [Fact]
    public void Emitter_DoWhile_BlockBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                var i = 0
                do
                {
                    if i > 3
                        break
                    if i > 5
                        continue
                    i = 1
                } while true
            }
            return
";
        const string il = @"
            IL0000: ldc.i4.0
            IL0001: stloc.0
            IL0002: ldloc.0
            IL0003: ldc.i4.3
            IL0004: cgt
            IL0006: brfalse.s IL_000a: ldloc.0
            IL0008: br.s IL_0018: ret
            IL000A: ldloc.0
            IL000B: ldc.i4.5
            IL000C: cgt
            IL000E: brfalse.s IL_0012: ldc.i4.1
            IL0010: br.s IL_0016: br.s IL_0002
            IL0012: ldc.i4.1
            IL0013: dup
            IL0014: stloc.0
            IL0015: pop
            IL0016: br.s IL_0002: ldloc.0
            IL0018: ret
";
        code.AssertIl("test", il);
    }
    [Fact]
    public void Emitter_DoWhile_SingleStatementBodyDebug()
    {
        const string code = @"
            function test(i:int) [{]
                do
                  [println("""")]
                [while i > 0]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldstr 
            IL0006: call System.Void System.Console::WriteLine(System.Object)
            IL000B: ldarg.0
            IL000C: ldc.i4.0
            IL000D: cgt
            IL000F: brtrue.s IL_0001: ldstr """"
            IL0011: br.s IL_0013: ret
            IL0013: ret
";
        code.AssertIlAndSymbols("test", il);
    }
    [Fact]
    public void Emitter_DoWhile_SingleStatementBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                do
                  println("""")
                while i > 0
            }
            return
";
        const string il = @"
IL0000: ldstr 
IL0005: call System.Void System.Console::WriteLine(System.Object)
IL000A: ldarg.0
IL000B: ldc.i4.0
IL000C: cgt
IL000E: brtrue.s IL_0000: ldstr """"
IL0010: ret
";
        code.AssertIl("test", il);
    }
}