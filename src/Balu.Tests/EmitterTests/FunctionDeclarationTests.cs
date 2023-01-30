using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_FunctionDclaration_VoidDebug()
    {
        const string code = @"
            function test(i:int) [{]
                [var a = i]
                [if i > 5] [return]
                [a = 2*i]
                [return]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: stloc.0
            IL0003: ldarg.0
            IL0004: ldc.i4.5
            IL0005: cgt
            IL0007: brfalse.s IL_000b: ldc.i4.2
            IL0009: br.s IL_0013: nop
            IL000B: ldc.i4.2
            IL000C: ldarg.0
            IL000D: mul
            IL000E: dup
            IL000F: stloc.0
            IL0010: pop
            IL0011: br.s IL_0013: nop
            IL0013: nop
            IL0014: ret
";
        var offsets = new[] { 0, 1, 3, 9, 0x0b, 0x11, 0x13 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_FunctionDclaration_VoidRelease()
    {
        const string code = @"
            function test(i:int) {
                var a = i
                if i > 5 return
                a = 2*i
                return
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: stloc.0
            IL0002: ldarg.0
            IL0003: ldc.i4.5
            IL0004: cgt
            IL0006: brfalse.s IL_0009: ldc.i4.2
            IL0008: ret
            IL0009: ldc.i4.2
            IL000A: ldarg.0
            IL000B: mul
            IL000C: dup
            IL000D: stloc.0
            IL000E: pop
            IL000F: ret
";

        code.AssertIl("test", il);
    }
    [Fact]
    public void Emitter_FunctionDclaration_IntDebug()
    {
        const string code = @"
            function test(i:int) : int [{]
                [var a = i]
                [if i > 5] [return a]
                [a = 2*i]
                [return a]
            [}]
            return
";
        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: stloc.0
            IL0003: ldarg.0
            IL0004: ldc.i4.5
            IL0005: cgt
            IL0007: brfalse.s IL_000c: ldc.i4.2
            IL0009: ldloc.0
            IL000A: br.s IL_0015: nop
            IL000C: ldc.i4.2
            IL000D: ldarg.0
            IL000E: mul
            IL000F: dup
            IL0010: stloc.0
            IL0011: pop
            IL0012: ldloc.0
            IL0013: br.s IL_0015: nop
            IL0015: nop
            IL0016: ret
";
        var offsets = new[] { 0, 1, 3, 9, 0xC, 0x12, 0x15 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_FunctionDclaration_IntRelease()
    {
        const string code = @"
            function test(i:int) : int {
                var a = i
                if i > 5 return a
                a = 2*i
                return a
            }
            return
";
        const string il = @"
            IL0000: ldarg.0
            IL0001: stloc.0
            IL0002: ldarg.0
            IL0003: ldc.i4.5
            IL0004: cgt
            IL0006: brfalse.s IL_000a: ldc.i4.2
            IL0008: ldloc.0
            IL0009: ret
            IL000A: ldc.i4.2
            IL000B: ldarg.0
            IL000C: mul
            IL000D: dup
            IL000E: stloc.0
            IL000F: pop
            IL0010: ldloc.0
            IL0011: ret
";
        code.AssertIl("test", il);
    }
}