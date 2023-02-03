using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_While_EmptyBlockBodyDebug()
    {
        const string code = @"
            function test(i:int) [{]
                [while i> 0]
                [{]
                [}]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: ldc.i4.0
            IL0003: cgt
            IL0005: brfalse.s IL_000b: nop
            IL0007: nop
            IL0008: nop
            IL0009: br.s IL_0001: ldarg.0
            IL000B: nop
            IL000C: ret
";
        var offsets = new[] { 0, 1, 7, 8, 0xB};
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_While_EmptyBlockBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                while i>0
                {
                }
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: ldc.i4.0
            IL0002: cgt
            IL0004: brfalse.s IL_0008: ret
            IL0006: br.s IL_0000: ldarg.0
            IL0008: ret
";
        code.AssertIl("test", il, output: output);
    }

    [Fact]
    public void Emitter_While_BlockBodyDebug()
    {
        const string code = @"
            function test(i:int) [{]
                [while i> 0]
                [{]
                    [if i > 3]
                        [break]
                    [if i > 5]
                        [continue]
                    [i = 1]
                [}]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: ldc.i4.0
            IL0003: cgt
            IL0005: brfalse.s IL_0020: nop
            IL0007: nop
            IL0008: ldarg.0
            IL0009: ldc.i4.3
            IL000A: cgt
            IL000C: brfalse.s IL_0010: ldarg.0
            IL000E: br.s IL_0020: nop
            IL0010: ldarg.0
            IL0011: ldc.i4.5
            IL0012: cgt
            IL0014: brfalse.s IL_0018: ldc.i4.1
            IL0016: br.s IL_0001: ldarg.0
            IL0018: ldc.i4.1
            IL0019: starg i
            IL001D: nop
            IL001E: br.s IL_0001: ldarg.0
            IL0020: nop
            IL0021: ret
";
        var offsets = new[] { 0, 1, 7, 8, 0xE, 0x10, 0x16, 0x18, 0x1D, 0x20 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_While_BlockBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                while i>0
                {
                    if i > 3
                        break
                    if i > 5
                        continue
                    i = 1
                }
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: ldc.i4.0
            IL0002: cgt
            IL0004: brfalse.s IL_001d: ret
            IL0006: ldarg.0
            IL0007: ldc.i4.3
            IL0008: cgt
            IL000A: brfalse.s IL_000e: ldarg.0
            IL000C: br.s IL_001d: ret
            IL000E: ldarg.0
            IL000F: ldc.i4.5
            IL0010: cgt
            IL0012: brfalse.s IL_0016: ldc.i4.1
            IL0014: br.s IL_0000: ldarg.0
            IL0016: ldc.i4.1
            IL0017: starg i
            IL001B: br.s IL_0000: ldarg.0
            IL001D: ret
";
        code.AssertIl("test", il, output: output);
    }

    [Fact]
    public void Emitter_While_SingleStatementBodyDebug()
    {
        const string code = @"
            function test(i:int) [{]
                [while i>0]
                  [println("""")]                
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: ldc.i4.0
            IL0003: cgt
            IL0005: brfalse.s IL_0013: nop
            IL0007: ldstr
            IL000C: call System.Void System.Console::WriteLine(System.Object)
            IL0011: br.s IL_0001: ldarg.0
            IL0013: nop
            IL0014: ret
";
        var offsets = new[] { 0, 1, 7, 0x13 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_While_SingleStatementBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                while i>0
                  println("""")
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: ldc.i4.0
            IL0002: cgt
            IL0004: brfalse.s IL_0012: ret
            IL0006: ldstr
            IL000B: call System.Void System.Console::WriteLine(System.Object)
            IL0010: br.s IL_0000: ldarg.0
            IL0012: ret
";
        code.AssertIl("test", il);
    }
}