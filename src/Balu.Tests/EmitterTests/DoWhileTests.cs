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
                do
                [{]
                    [if i > 3]
                        [break]
                    [if i > 5]
                        [continue]
                    [i = 1]
                [}] [while i> 0]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: nop
            IL0002: ldarg.0
            IL0003: ldc.i4.3
            IL0004: cgt
            IL0006: brfalse.s IL_000a: ldarg.0
            IL0008: br.s IL_0020: br.s IL_0022
            IL000A: ldarg.0
            IL000B: ldc.i4.5
            IL000C: cgt
            IL000E: brfalse.s IL_0012: ldc.i4.1
            IL0010: br.s IL_001a: ldarg.0
            IL0012: ldc.i4.1
            IL0013: dup
            IL0014: starg i
            IL0018: pop
            IL0019: nop
            IL001A: ldarg.0
            IL001B: ldc.i4.0
            IL001C: cgt
            IL001E: brtrue.s IL_0001: nop
            IL0020: br.s IL_0022: nop
            IL0022: nop
            IL0023: ret
";
        var offsets = new[] { 0, 1, 2, 8, 0x0A, 0x10, 0x12, 0x19, 0x1A, 0x22 };
        code.AssertIlAndSymbols("test", il, offsets);
    }
    [Fact]
    public void Emitter_DoWhile_BlockBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                do
                {
                    if i > 3
                        break
                    if i > 5
                        continue
                    i = 1
                } while i > 0
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: ldc.i4.3
            IL0002: cgt
            IL0004: brfalse.s IL_0008: ldarg.0
            IL0006: br.s IL_001d: ret
            IL0008: ldarg.0
            IL0009: ldc.i4.5
            IL000A: cgt
            IL000C: brfalse.s IL_0010: ldc.i4.1
            IL000E: br.s IL_0017: ldarg.0
            IL0010: ldc.i4.1
            IL0011: dup
            IL0012: starg i
            IL0016: pop
            IL0017: ldarg.0
            IL0018: ldc.i4.0
            IL0019: cgt
            IL001B: brtrue.s IL_0000: ldarg.0
            IL001D: ret
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
                IL0011: br.s IL_0013: nop
                IL0013: nop
                IL0014: ret
";
        var offsets = new[] { 0, 1, 0x0B, 0x13 };
        code.AssertIlAndSymbols("test", il, offsets);
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