using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
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
            IL0006: br.s IL_000e: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: dup
            IL000C: stloc.0
            IL000D: pop
            IL000E: ldloc.0
            IL000F: ldc.i4.s 10
            IL0011: cgt
            IL0013: ldc.i4.0
            IL0014: ceq
            IL0016: brfalse.s IL_0030: nop
            IL0018: nop
            IL0019: ldloc.0
            IL001A: ldc.i4.3
            IL001B: cgt
            IL001D: brfalse.s IL_0021: ldloc.0
            IL001F: br.s IL_0030: nop
            IL0021: ldloc.0
            IL0022: ldc.i4.5
            IL0023: cgt
            IL0025: brfalse.s IL_0029: ldc.i4.1
            IL0027: br.s IL_0008: ldloc.0
            IL0029: ldc.i4.1
            IL002A: dup
            IL002B: stloc.0
            IL002C: pop
            IL002D: nop
            IL002E: br.s IL_0008: ldloc.0
            IL0030: nop
            IL0031: ret
";
        var offsets = new[] { 0,1,3,8,0xE, 0x18, 0x19, 0x1F, 0x21, 0x27, 0x29, 0x2D, 0x30 };
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
            IL0005: br.s IL_000d: ldloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: dup
            IL000B: stloc.0
            IL000C: pop
            IL000D: ldloc.0
            IL000E: ldc.i4.s 10
            IL0010: cgt
            IL0012: ldc.i4.0
            IL0013: ceq
            IL0015: brfalse.s IL_002d: ret
            IL0017: ldloc.0
            IL0018: ldc.i4.3
            IL0019: cgt
            IL001B: brfalse.s IL_001f: ldloc.0
            IL001D: br.s IL_002d: ret
            IL001F: ldloc.0
            IL0020: ldc.i4.5
            IL0021: cgt
            IL0023: brfalse.s IL_0027: ldc.i4.1
            IL0025: br.s IL_0007: ldloc.0
            IL0027: ldc.i4.1
            IL0028: dup
            IL0029: stloc.0
            IL002A: pop
            IL002B: br.s IL_0007: ldloc.0
            IL002D: ret
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
            IL0006: br.s IL_000e: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: dup
            IL000C: stloc.0
            IL000D: pop
            IL000E: ldloc.0
            IL000F: ldc.i4.s 10
            IL0011: cgt
            IL0013: ldc.i4.0
            IL0014: ceq
            IL0016: brfalse.s IL_0024: nop
            IL0018: ldstr
            IL001D: call System.Void System.Console::WriteLine(System.Object)
            IL0022: br.s IL_0008: ldloc.0
            IL0024: nop
            IL0025: ret
";
        var offsets = new[] { 0, 1, 3, 8, 0xE, 0x18, 0x24 };
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
            IL0005: br.s IL_000d: ldloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: dup
            IL000B: stloc.0
            IL000C: pop
            IL000D: ldloc.0
            IL000E: ldc.i4.s 10
            IL0010: cgt
            IL0012: ldc.i4.0
            IL0013: ceq
            IL0015: brfalse.s IL_0023: ret
            IL0017: ldstr
            IL001C: call System.Void System.Console::WriteLine(System.Object)
            IL0021: br.s IL_0007: ldloc.0
            IL0023: ret
";
        code.AssertIl("test", il, output: output);
    }
}