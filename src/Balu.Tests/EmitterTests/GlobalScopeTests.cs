using Balu.Symbols;
using Balu.Tests.TestHelper;
using System;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_GlobalScope_EmptyScriptDebug()
    {
        const string code = "";
        const string il = @"
            IL0000: ldsfld System.Object Program::<result>
            IL0005: nop
            IL0006: ret
";
        code.AssertIlAndSymbols("<eval>", il, Array.Empty<int>(), script: true);
    }
    [Fact]
    public void Emitter_GlobalScope_EmptyScriptRelease()
    {
        const string code = "";
        const string il = @"
            IL0000: ldsfld System.Object Program::<result>
            IL0005: ret
";
        code.AssertIl(GlobalSymbolNames.Eval, il, script: true);
    }
    [Fact]
    public void Emitter_GlobalScope_DebugNoScript()
    {
        const string code = @"[ ]
            [var a = 1]
            [a = 2*a]
            [if a > 2] [return]
            [println(string(a)[)]]";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stsfld System.Int32 Program::a
            IL0007: ldc.i4.2
            IL0008: ldsfld System.Int32 Program::a
            IL000D: mul
            IL000E: stsfld System.Int32 Program::a
            IL0013: ldsfld System.Int32 Program::a
            IL0018: ldc.i4.2
            IL0019: cgt
            IL001B: brfalse.s IL_001f: ldsfld System.Int32 Program::a
            IL001D: br.s IL_0033: nop
            IL001F: ldsfld System.Int32 Program::a
            IL0024: box System.Int32
            IL0029: call System.String System.Convert::ToString(System.Object)
            IL002E: call System.Void System.Console::WriteLine(System.Object)
            IL0033: nop
            IL0034: ret
";
        var offsets = new[] { 0, 1, 7, 0x13, 0x1D, 0x1F, 0x33 };
        code.AssertIlAndSymbols("main", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_GlobalScope_ReleaseNoScript()
    {
        const string code = @"
            var a = 1
            a = 2*a
            if a > 2 return
            println(string(a))
";
        const string il = @"
            IL0000: ldc.i4.1
            IL0001: stsfld System.Int32 Program::a
            IL0006: ldc.i4.2
            IL0007: ldsfld System.Int32 Program::a
            IL000C: mul
            IL000D: stsfld System.Int32 Program::a
            IL0012: ldsfld System.Int32 Program::a
            IL0017: ldc.i4.2
            IL0018: cgt
            IL001A: brfalse.s IL_001d: ldsfld System.Int32 Program::a
            IL001C: ret
            IL001D: ldsfld System.Int32 Program::a
            IL0022: box System.Int32
            IL0027: call System.String System.Convert::ToString(System.Object)
            IL002C: call System.Void System.Console::WriteLine(System.Object)
            IL0031: ret
";
        code.AssertIl("main", il, output: output);
    }
    [Fact]
    public void Emitter_GlobalScope_DebugScript()
    {
        const string code = @"[ ]
            [var a = 1]
            [a = 2*a]
            [if a > 2] [return a]
            [println(string(a))]
[ ]";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stsfld System.Int32 Program::a
            IL0007: ldc.i4.2
            IL0008: ldsfld System.Int32 Program::a
            IL000D: mul
            IL000E: dup
            IL000F: stsfld System.Int32 Program::a
            IL0014: box System.Int32
            IL0019: stsfld System.Object Program::<result>
            IL001E: ldsfld System.Int32 Program::a
            IL0023: ldc.i4.2
            IL0024: cgt
            IL0026: brfalse.s IL_0034: ldsfld System.Int32 Program::a
            IL0028: ldsfld System.Int32 Program::a
            IL002D: box System.Int32
            IL0032: br.s IL_004d: nop
            IL0034: ldsfld System.Int32 Program::a
            IL0039: box System.Int32
            IL003E: call System.String System.Convert::ToString(System.Object)
            IL0043: call System.Void System.Console::WriteLine(System.Object)
            IL0048: ldsfld System.Object Program::<result>
            IL004D: nop
            IL004E: ret
";
        var offsets = new[] { 0, 1, 7, 0x1E, 0x28, 0x34, 0x4D };
        code.AssertIlAndSymbols(GlobalSymbolNames.Eval, il, offsets, script: true, output: output);
    }
    [Fact]
    public void Emitter_GlobalScope_ReleaseScript()
    {
        const string code = @"
            var a = 1
            a = 2*a
            if a > 2 return a
            println(string(a))
";
        const string il = @"
            IL0000: ldc.i4.1
            IL0001: stsfld System.Int32 Program::a
            IL0006: ldc.i4.2
            IL0007: ldsfld System.Int32 Program::a
            IL000C: mul
            IL000D: dup
            IL000E: stsfld System.Int32 Program::a
            IL0013: box System.Int32
            IL0018: stsfld System.Object Program::<result>
            IL001D: ldsfld System.Int32 Program::a
            IL0022: ldc.i4.2
            IL0023: cgt
            IL0025: brfalse.s IL_0032: ldsfld System.Int32 Program::a
            IL0027: ldsfld System.Int32 Program::a
            IL002C: box System.Int32
            IL0031: ret
            IL0032: ldsfld System.Int32 Program::a
            IL0037: box System.Int32
            IL003C: call System.String System.Convert::ToString(System.Object)
            IL0041: call System.Void System.Console::WriteLine(System.Object)
            IL0046: ldsfld System.Object Program::<result>
            IL004B: ret
";
        code.AssertIl(GlobalSymbolNames.Eval, il, script: true, output: output);
    }

}