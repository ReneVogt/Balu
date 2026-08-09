using TestHelpers;
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
            IL0006: br.s IL_0016: ldloc.0
            IL0008: ldc.i4.s 10
            IL000A: ldloc.0
            IL000B: cgt
            IL000D: ldc.i4.0
            IL000E: ceq
            IL0010: brtrue.s IL_0024: nop
            IL0012: ldloc.0
            IL0013: ldc.i4.1
            IL0014: add
            IL0015: stloc.0
            IL0016: ldloc.0
            IL0017: ldc.i4.s 10
            IL0019: cgt
            IL001B: ldc.i4.0
            IL001C: ceq
            IL001E: brfalse.s IL_0024: nop
            IL0020: nop
            IL0021: nop
            IL0022: br.s IL_0008: ldc.i4.s 10
            IL0024: nop
            IL0025: ret
";
        var offsets = new[] { 0, 1, 3, 0x12, 0x16, 0x20, 0x21, 0x24 };
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
            IL0005: br.s IL_0015: ldloc.0
            IL0007: ldc.i4.s 10
            IL0009: ldloc.0
            IL000A: cgt
            IL000C: ldc.i4.0
            IL000D: ceq
            IL000F: brtrue.s IL_0021: ret
            IL0011: ldloc.0
            IL0012: ldc.i4.1
            IL0013: add
            IL0014: stloc.0
            IL0015: ldloc.0
            IL0016: ldc.i4.s 10
            IL0018: cgt
            IL001A: ldc.i4.0
            IL001B: ceq
            IL001D: brfalse.s IL_0021: ret
            IL001F: br.s IL_0007: ldc.i4.s 10
            IL0021: ret
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
            IL0006: br.s IL_0016: ldloc.0
            IL0008: ldc.i4.s 10
            IL000A: ldloc.0
            IL000B: cgt
            IL000D: ldc.i4.0
            IL000E: ceq
            IL0010: brtrue.s IL_0036: nop
            IL0012: ldloc.0
            IL0013: ldc.i4.1
            IL0014: add
            IL0015: stloc.0
            IL0016: ldloc.0
            IL0017: ldc.i4.s 10
            IL0019: cgt
            IL001B: ldc.i4.0
            IL001C: ceq
            IL001E: brfalse.s IL_0036: nop
            IL0020: nop
            IL0021: ldloc.0
            IL0022: ldc.i4.3
            IL0023: cgt
            IL0025: brfalse.s IL_0029: ldloc.0
            IL0027: br.s IL_0036: nop
            IL0029: ldloc.0
            IL002A: ldc.i4.5
            IL002B: cgt
            IL002D: brfalse.s IL_0031: ldc.i4.1
            IL002F: br.s IL_0008: ldc.i4.s 10
            IL0031: ldc.i4.1
            IL0032: stloc.0
            IL0033: nop
            IL0034: br.s IL_0008: ldc.i4.s 10
            IL0036: nop
            IL0037: ret
";
        var offsets = new[] { 0, 1, 3, 0x12, 0x16, 0x20, 0x21, 0x27, 0x29, 0x2F, 0x31, 0x33, 0x36 };
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
            IL0005: br.s IL_0015: ldloc.0
            IL0007: ldc.i4.s 10
            IL0009: ldloc.0
            IL000A: cgt
            IL000C: ldc.i4.0
            IL000D: ceq
            IL000F: brtrue.s IL_0033: ret
            IL0011: ldloc.0
            IL0012: ldc.i4.1
            IL0013: add
            IL0014: stloc.0
            IL0015: ldloc.0
            IL0016: ldc.i4.s 10
            IL0018: cgt
            IL001A: ldc.i4.0
            IL001B: ceq
            IL001D: brfalse.s IL_0033: ret
            IL001F: ldloc.0
            IL0020: ldc.i4.3
            IL0021: cgt
            IL0023: brfalse.s IL_0027: ldloc.0
            IL0025: br.s IL_0033: ret
            IL0027: ldloc.0
            IL0028: ldc.i4.5
            IL0029: cgt
            IL002B: brfalse.s IL_002f: ldc.i4.1
            IL002D: br.s IL_0007: ldc.i4.s 10
            IL002F: ldc.i4.1
            IL0030: stloc.0
            IL0031: br.s IL_0007: ldc.i4.s 10
            IL0033: ret
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
            IL0006: br.s IL_0016: ldloc.0
            IL0008: ldc.i4.s 10
            IL000A: ldloc.0
            IL000B: cgt
            IL000D: ldc.i4.0
            IL000E: ceq
            IL0010: brtrue.s IL_002c: nop
            IL0012: ldloc.0
            IL0013: ldc.i4.1
            IL0014: add
            IL0015: stloc.0
            IL0016: ldloc.0
            IL0017: ldc.i4.s 10
            IL0019: cgt
            IL001B: ldc.i4.0
            IL001C: ceq
            IL001E: brfalse.s IL_002c: nop
            IL0020: ldstr
            IL0025: call System.Void System.Console::WriteLine(System.Object)
            IL002A: br.s IL_0008: ldc.i4.s 10
            IL002C: nop
            IL002D: ret
";
        var offsets = new[] { 0, 1, 3, 0x12, 0x16, 0x20, 0x2C };
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
            IL0005: br.s IL_0015: ldloc.0
            IL0007: ldc.i4.s 10
            IL0009: ldloc.0
            IL000A: cgt
            IL000C: ldc.i4.0
            IL000D: ceq
            IL000F: brtrue.s IL_002b: ret
            IL0011: ldloc.0
            IL0012: ldc.i4.1
            IL0013: add
            IL0014: stloc.0
            IL0015: ldloc.0
            IL0016: ldc.i4.s 10
            IL0018: cgt
            IL001A: ldc.i4.0
            IL001B: ceq
            IL001D: brfalse.s IL_002b: ret
            IL001F: ldstr
            IL0024: call System.Void System.Console::WriteLine(System.Object)
            IL0029: br.s IL_0007: ldc.i4.s 10
            IL002B: ret
";
        code.AssertIl("test", il, output: output);
    }
}
