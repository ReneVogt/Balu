﻿using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_IfStatement_SimpleCorrectSequencePoints()
    {
        const string code = @"
        function back(){}
        function test(i:int) [{]
            [if i < 10]            
                [back()]           
        [}]
        test(12)
";
        var sequencePointOffsets = new[] { 0, 1, 8, 15 };

        const string il = @"
            IL0000: nop
            IL0001: ldarg.0
            IL0002: ldc.i4.s 10
            IL0004: clt
            IL0006: brfalse.s IL_000d: br.s IL_000f
            IL0008: call System.Void Program::back()
            IL000D: br.s IL_000f: ret
            IL000F: ret
";
        code.AssertIlAndSymbols("test", il, sequencePointOffsets: sequencePointOffsets);
    }
    [Fact]
    public void Emitter_IfStatement_SimpleWithReturnCorrectSequencePoints()
    {
        const string code = @"
        function back(){}
        function test(i:int) [{]
            [if i < 10]            
                [back()]
            [return]
        [}]
        test(12)
";
        const string il = @"
IL0000: nop
            IL0001: ldarg.0
            IL0002: ldc.i4.s 10
            IL0004: clt
            IL0006: brfalse.s IL_000d: br.s IL_000f
            IL0008: call System.Void Program::back()
            IL000D: br.s IL_000f: ret
            IL000F: ret
";
        var sequencePointOffsets = new[] { 0, 1, 8, 13, 15 };
        code.AssertIlAndSymbols("test", il, sequencePointOffsets: sequencePointOffsets);
    }

    [Fact]
    public void Emitter_IfStatement_NoElseSymbolsBlock()
    {
        const string code = @"
        function test(i:int) [{]
            [var a = 0]
            [if i < 10]
            [{]
                [a = 10]
            [}]
        [}]
        test(12)
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: ldarg.0
            IL0004: ldc.i4.s 10
            IL0006: clt
            IL0008: brfalse.s IL_0011: br.s IL_0013
            IL000A: nop
            IL000B: ldc.i4.s 10
            IL000D: dup
            IL000E: stloc.0
            IL000F: pop
            IL0010: nop
            IL0011: br.s IL_0013: ret
            IL0013: ret
";
        var offsets = new[] { 0, 1, 3, 0xA, 0xB, 0x10, 0x13 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_IfStatement_SymbolsBlock()
    {
        const string code = @"
        function test(i:int) [{]
            [var a = 0]
            [if i < 10]
            [{]
                [a = 10]
            [}]
            else
            [{]
                [a = 0]
            [}]
        [}]
        return
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: ldarg.0
            IL0004: ldc.i4.s 10
            IL0006: clt
            IL0008: brfalse.s IL_0013: nop
            IL000A: nop
            IL000B: ldc.i4.s 10
            IL000D: dup
            IL000E: stloc.0
            IL000F: pop
            IL0010: nop
            IL0011: br.s IL_0019: br.s IL_001b
            IL0013: nop
            IL0014: ldc.i4.0
            IL0015: dup
            IL0016: stloc.0
            IL0017: pop
            IL0018: nop
            IL0019: br.s IL_001b: ret
            IL001B: ret
";
        var offsets = new[] { 0, 1, 3, 0xA, 0xB, 0x10, 0x13, 0x14, 0x18, 0x1B };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_IfStatement_SymbolsNoBlock()
    {
        const string code = @"
        function test(i:int) [{]
            [var a = 0]
            [if i < 10]
                [a = 10]
            else
                [a = 0]
        [}]
        return
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: ldarg.0
            IL0004: ldc.i4.s 10
            IL0006: clt
            IL0008: brfalse.s IL_0011: ldc.i4.0
            IL000A: ldc.i4.s 10
            IL000C: dup
            IL000D: stloc.0
            IL000E: pop
            IL000F: br.s IL_0015: br.s IL_0017
            IL0011: ldc.i4.0
            IL0012: dup
            IL0013: stloc.0
            IL0014: pop
            IL0015: br.s IL_0017: ret
            IL0017: ret
";
        var offsets = new[] { 0, 1, 3, 0xA, 0x11, 0x17 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }

}