using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_For_EmptyBlockBodyDebug()
    {
        const string code = @"
            function test() [{]
                for [i = 1] [[to] [10]]
                [{]
                [}]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stloc.0
            IL0003: ldc.i4.s 10
            IL0005: stloc.1
            IL0006: br.s IL_000c: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: stloc.0
            IL000C: ldloc.0
            IL000D: ldc.i4.s 10
            IL000F: cgt
            IL0011: ldc.i4.0
            IL0012: ceq
            IL0014: brfalse.s IL_001a: nop
            IL0016: nop
            IL0017: nop
            IL0018: br.s IL_0008: ldloc.0
            IL001A: nop
            IL001B: ret
";
        var offsets = new[] { 0, 1, 3, 8, 0xC, 0x16, 0x17, 0x1A };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_For_EmptyBlockBodyRelease()
    {
        const string code = @"
            function test() {
                for i = 1 to 10
                {
                }
            }
            return
";
        const string il = @"
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldc.i4.s 10
            IL0004: stloc.1
            IL0005: br.s IL_000b: ldloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: stloc.0
            IL000B: ldloc.0
            IL000C: ldc.i4.s 10
            IL000E: cgt
            IL0010: ldc.i4.0
            IL0011: ceq
            IL0013: brfalse.s IL_0017: ret
            IL0015: br.s IL_0007: ldloc.0
            IL0017: ret
";
        code.AssertIl("test", il, output: output);
    }

    [Fact]
    public void Emitter_For_BlockBodyDebug()
    {
        const string code = @"
            function test() [{]
                for [i = 1] [[to] [10]]
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
            IL0001: ldc.i4.1
            IL0002: stloc.0
            IL0003: ldc.i4.s 10
            IL0005: stloc.1
            IL0006: br.s IL_000c: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: stloc.0
            IL000C: ldloc.0
            IL000D: ldc.i4.s 10
            IL000F: cgt
            IL0011: ldc.i4.0
            IL0012: ceq
            IL0014: brfalse.s IL_002c: nop
            IL0016: nop
            IL0017: ldloc.0
            IL0018: ldc.i4.3
            IL0019: cgt
            IL001B: brfalse.s IL_001f: ldloc.0
            IL001D: br.s IL_002c: nop
            IL001F: ldloc.0
            IL0020: ldc.i4.5
            IL0021: cgt
            IL0023: brfalse.s IL_0027: ldc.i4.1
            IL0025: br.s IL_0008: ldloc.0
            IL0027: ldc.i4.1
            IL0028: stloc.0
            IL0029: nop
            IL002A: br.s IL_0008: ldloc.0
            IL002C: nop
            IL002D: ret
";
        var offsets = new[] { 0,1,3,8,0xC, 0x16, 0x17, 0x1D, 0x1F, 0x25, 0x27, 0x29, 0x2C };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_For_BlockBodyRelease()
    {
        const string code = @"
            function test() {
                for i = 1 to 10
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
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldc.i4.s 10
            IL0004: stloc.1
            IL0005: br.s IL_000b: ldloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: stloc.0
            IL000B: ldloc.0
            IL000C: ldc.i4.s 10
            IL000E: cgt
            IL0010: ldc.i4.0
            IL0011: ceq
            IL0013: brfalse.s IL_0029: ret
            IL0015: ldloc.0
            IL0016: ldc.i4.3
            IL0017: cgt
            IL0019: brfalse.s IL_001d: ldloc.0
            IL001B: br.s IL_0029: ret
            IL001D: ldloc.0
            IL001E: ldc.i4.5
            IL001F: cgt
            IL0021: brfalse.s IL_0025: ldc.i4.1
            IL0023: br.s IL_0007: ldloc.0
            IL0025: ldc.i4.1
            IL0026: stloc.0
            IL0027: br.s IL_0007: ldloc.0
            IL0029: ret
";
        code.AssertIl("test", il, output: output);
    }

    [Fact]
    public void Emitter_For_SingleStatementBodyDebug()
    {
        const string code = @"
            function test() [{]
                for [i = 1] [[to] [10]]
                  [println("""")]                
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stloc.0
            IL0003: ldc.i4.s 10
            IL0005: stloc.1
            IL0006: br.s IL_000c: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: stloc.0
            IL000C: ldloc.0
            IL000D: ldc.i4.s 10
            IL000F: cgt
            IL0011: ldc.i4.0
            IL0012: ceq
            IL0014: brfalse.s IL_0022: nop
            IL0016: ldstr
            IL001B: call System.Void System.Console::WriteLine(System.Object)
            IL0020: br.s IL_0008: ldloc.0
            IL0022: nop
            IL0023: ret
";
        var offsets = new[] { 0, 1, 3, 8, 0xC, 0x16, 0x22 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_For_SingleStatementBodyRelease()
    {
        const string code = @"
            function test(i:int) {
                for i = 1 to 10
                  println("""")
            }
            return
";
        const string il = @"
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldc.i4.s 10
            IL0004: stloc.1
            IL0005: br.s IL_000b: ldloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: stloc.0
            IL000B: ldloc.0
            IL000C: ldc.i4.s 10
            IL000E: cgt
            IL0010: ldc.i4.0
            IL0011: ceq
            IL0013: brfalse.s IL_0021: ret
            IL0015: ldstr
            IL001A: call System.Void System.Console::WriteLine(System.Object)
            IL001F: br.s IL_0007: ldloc.0
            IL0021: ret
";
        code.AssertIl("test", il, output: output);
    }
}