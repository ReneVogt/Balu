using Balu.Symbols;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_Assignments_DebugCorrectHandlingOfStatementsVsExpressions()
    {
        const string code = @"
            function test()
            [{]
                [var a=0]
                [var b=0]
                [a = 12]
                [a++]
                [++a]
                [a--]
                [--a]
                [b = a = 5]
                [b = a++]
                [b = ++a]
                [b = a--]
                [b = --a]
            [}]
            test()
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: ldc.i4.0
            IL0004: stloc.1
            IL0005: ldc.i4.s 12
            IL0007: stloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: stloc.0
            IL000C: ldloc.0
            IL000D: ldc.i4.1
            IL000E: add
            IL000F: stloc.0
            IL0010: ldloc.0
            IL0011: ldc.i4.1
            IL0012: sub
            IL0013: stloc.0
            IL0014: ldloc.0
            IL0015: ldc.i4.1
            IL0016: sub
            IL0017: stloc.0
            IL0018: ldc.i4.5
            IL0019: dup
            IL001A: stloc.0
            IL001B: stloc.1
            IL001C: ldloc.0
            IL001D: dup
            IL001E: ldc.i4.1
            IL001F: add
            IL0020: stloc.0
            IL0021: stloc.1
            IL0022: ldloc.0
            IL0023: ldc.i4.1
            IL0024: add
            IL0025: dup
            IL0026: stloc.0
            IL0027: stloc.1
            IL0028: ldloc.0
            IL0029: dup
            IL002A: ldc.i4.1
            IL002B: sub
            IL002C: stloc.0
            IL002D: stloc.1
            IL002E: ldloc.0
            IL002F: ldc.i4.1
            IL0030: sub
            IL0031: dup
            IL0032: stloc.0
            IL0033: stloc.1
            IL0034: nop
            IL0035: ret
";
        var offsets = new[] { 0, 1, 3, 5, 8, 0xC, 0x10, 0x14, 0x18, 0x1C, 0x22, 0x28, 0x2E, 0x34 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_Assignments_ReleaseCorrectHandlingOfStatementsVsExpressions()
    {
        const string code = @"
            function test()
            {
                var a=0
                var b=0
                a = 12
                a++
                ++a
                a--
                --a
                b = a = 5
                b = a++
                b = ++a
                b = a--
                b = --a
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.0
            IL0001: stloc.0
            IL0002: ldc.i4.0
            IL0003: stloc.1
            IL0004: ldc.i4.s 12
            IL0006: stloc.0
            IL0007: ldloc.0
            IL0008: ldc.i4.1
            IL0009: add
            IL000A: stloc.0
            IL000B: ldloc.0
            IL000C: ldc.i4.1
            IL000D: add
            IL000E: stloc.0
            IL000F: ldloc.0
            IL0010: ldc.i4.1
            IL0011: sub
            IL0012: stloc.0
            IL0013: ldloc.0
            IL0014: ldc.i4.1
            IL0015: sub
            IL0016: stloc.0
            IL0017: ldc.i4.5
            IL0018: dup
            IL0019: stloc.0
            IL001A: stloc.1
            IL001B: ldloc.0
            IL001C: dup
            IL001D: ldc.i4.1
            IL001E: add
            IL001F: stloc.0
            IL0020: stloc.1
            IL0021: ldloc.0
            IL0022: ldc.i4.1
            IL0023: add
            IL0024: dup
            IL0025: stloc.0
            IL0026: stloc.1
            IL0027: ldloc.0
            IL0028: dup
            IL0029: ldc.i4.1
            IL002A: sub
            IL002B: stloc.0
            IL002C: stloc.1
            IL002D: ldloc.0
            IL002E: ldc.i4.1
            IL002F: sub
            IL0030: dup
            IL0031: stloc.0
            IL0032: stloc.1
            IL0033: ret
";
        code.AssertIl("test", il, output: output);
    }
    [Fact]
    public void Emitter_Assignments_DebugScriptCorrectHandlingOfStatementsVsExpressions()
    {
        const string code = @"
            [[v]ar a=0]
            [var b=0]
            [a = 12]
            [a++]
            [++a]
            [a--]
            [--a]
            [b = a = 5]
            [b = a++]
            [b = ++a]
            [b = a--]
            [b = --[a]]
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stsfld System.Int32 Program::a
            IL0007: ldc.i4.0
            IL0008: stsfld System.Int32 Program::b
            IL000D: ldc.i4.s 12
            IL000F: dup
            IL0010: stsfld System.Int32 Program::a
            IL0015: box System.Int32
            IL001A: stsfld System.Object Program::<result>
            IL001F: ldsfld System.Int32 Program::a
            IL0024: dup
            IL0025: ldc.i4.1
            IL0026: add
            IL0027: stsfld System.Int32 Program::a
            IL002C: box System.Int32
            IL0031: stsfld System.Object Program::<result>
            IL0036: ldsfld System.Int32 Program::a
            IL003B: ldc.i4.1
            IL003C: add
            IL003D: dup
            IL003E: stsfld System.Int32 Program::a
            IL0043: box System.Int32
            IL0048: stsfld System.Object Program::<result>
            IL004D: ldsfld System.Int32 Program::a
            IL0052: dup
            IL0053: ldc.i4.1
            IL0054: sub
            IL0055: stsfld System.Int32 Program::a
            IL005A: box System.Int32
            IL005F: stsfld System.Object Program::<result>
            IL0064: ldsfld System.Int32 Program::a
            IL0069: ldc.i4.1
            IL006A: sub
            IL006B: dup
            IL006C: stsfld System.Int32 Program::a
            IL0071: box System.Int32
            IL0076: stsfld System.Object Program::<result>
            IL007B: ldc.i4.5
            IL007C: dup
            IL007D: stsfld System.Int32 Program::a
            IL0082: dup
            IL0083: stsfld System.Int32 Program::b
            IL0088: box System.Int32
            IL008D: stsfld System.Object Program::<result>
            IL0092: ldsfld System.Int32 Program::a
            IL0097: dup
            IL0098: ldc.i4.1
            IL0099: add
            IL009A: stsfld System.Int32 Program::a
            IL009F: dup
            IL00A0: stsfld System.Int32 Program::b
            IL00A5: box System.Int32
            IL00AA: stsfld System.Object Program::<result>
            IL00AF: ldsfld System.Int32 Program::a
            IL00B4: ldc.i4.1
            IL00B5: add
            IL00B6: dup
            IL00B7: stsfld System.Int32 Program::a
            IL00BC: dup
            IL00BD: stsfld System.Int32 Program::b
            IL00C2: box System.Int32
            IL00C7: stsfld System.Object Program::<result>
            IL00CC: ldsfld System.Int32 Program::a
            IL00D1: dup
            IL00D2: ldc.i4.1
            IL00D3: sub
            IL00D4: stsfld System.Int32 Program::a
            IL00D9: dup
            IL00DA: stsfld System.Int32 Program::b
            IL00DF: box System.Int32
            IL00E4: stsfld System.Object Program::<result>
            IL00E9: ldsfld System.Int32 Program::a
            IL00EE: ldc.i4.1
            IL00EF: sub
            IL00F0: dup
            IL00F1: stsfld System.Int32 Program::a
            IL00F6: dup
            IL00F7: stsfld System.Int32 Program::b
            IL00FC: box System.Int32
            IL0101: stsfld System.Object Program::<result>
            IL0106: ldsfld System.Object Program::<result>
            IL010B: nop
            IL010C: ret
";
        var offsets = new[] { 1, 0, 7, 0xD, 0x1F, 0x36, 0x4D, 0x64, 0x7B, 0x92, 0xAF, 0xCC, 0xE9, 0x10B};
        code.AssertIlAndSymbols(GlobalSymbolNames.Eval, il, offsets, script: true, output: output);
    }
    [Fact]
    public void Emitter_Assignments_ReleaseScriptCorrectHandlingOfStatementsVsExpressions()
    {
        const string code = @"
            var a=0
            var b=0
            a = 12
            a++
            ++a
            a--
            --a
            b = a = 5
            b = a++
            b = ++a
            b = a--
            b = --a
";
        const string il = @"
            IL0000: ldc.i4.0
            IL0001: stsfld System.Int32 Program::a
            IL0006: ldc.i4.0
            IL0007: stsfld System.Int32 Program::b
            IL000C: ldc.i4.s 12
            IL000E: dup
            IL000F: stsfld System.Int32 Program::a
            IL0014: box System.Int32
            IL0019: stsfld System.Object Program::<result>
            IL001E: ldsfld System.Int32 Program::a
            IL0023: dup
            IL0024: ldc.i4.1
            IL0025: add
            IL0026: stsfld System.Int32 Program::a
            IL002B: box System.Int32
            IL0030: stsfld System.Object Program::<result>
            IL0035: ldsfld System.Int32 Program::a
            IL003A: ldc.i4.1
            IL003B: add
            IL003C: dup
            IL003D: stsfld System.Int32 Program::a
            IL0042: box System.Int32
            IL0047: stsfld System.Object Program::<result>
            IL004C: ldsfld System.Int32 Program::a
            IL0051: dup
            IL0052: ldc.i4.1
            IL0053: sub
            IL0054: stsfld System.Int32 Program::a
            IL0059: box System.Int32
            IL005E: stsfld System.Object Program::<result>
            IL0063: ldsfld System.Int32 Program::a
            IL0068: ldc.i4.1
            IL0069: sub
            IL006A: dup
            IL006B: stsfld System.Int32 Program::a
            IL0070: box System.Int32
            IL0075: stsfld System.Object Program::<result>
            IL007A: ldc.i4.5
            IL007B: dup
            IL007C: stsfld System.Int32 Program::a
            IL0081: dup
            IL0082: stsfld System.Int32 Program::b
            IL0087: box System.Int32
            IL008C: stsfld System.Object Program::<result>
            IL0091: ldsfld System.Int32 Program::a
            IL0096: dup
            IL0097: ldc.i4.1
            IL0098: add
            IL0099: stsfld System.Int32 Program::a
            IL009E: dup
            IL009F: stsfld System.Int32 Program::b
            IL00A4: box System.Int32
            IL00A9: stsfld System.Object Program::<result>
            IL00AE: ldsfld System.Int32 Program::a
            IL00B3: ldc.i4.1
            IL00B4: add
            IL00B5: dup
            IL00B6: stsfld System.Int32 Program::a
            IL00BB: dup
            IL00BC: stsfld System.Int32 Program::b
            IL00C1: box System.Int32
            IL00C6: stsfld System.Object Program::<result>
            IL00CB: ldsfld System.Int32 Program::a
            IL00D0: dup
            IL00D1: ldc.i4.1
            IL00D2: sub
            IL00D3: stsfld System.Int32 Program::a
            IL00D8: dup
            IL00D9: stsfld System.Int32 Program::b
            IL00DE: box System.Int32
            IL00E3: stsfld System.Object Program::<result>
            IL00E8: ldsfld System.Int32 Program::a
            IL00ED: ldc.i4.1
            IL00EE: sub
            IL00EF: dup
            IL00F0: stsfld System.Int32 Program::a
            IL00F5: dup
            IL00F6: stsfld System.Int32 Program::b
            IL00FB: box System.Int32
            IL0100: stsfld System.Object Program::<result>
            IL0105: ldsfld System.Object Program::<result>
            IL010A: ret
";
        code.AssertIl(GlobalSymbolNames.Eval, il, script: true, output: output);
    }
}