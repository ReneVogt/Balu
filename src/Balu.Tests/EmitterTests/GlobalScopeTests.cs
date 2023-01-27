using Balu.Tests.TestHelper;
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
            IL0005: br.s IL_0007: ret
            IL0007: ret
";
        code.AssertIlAndSymbols("<eval>", il, script: true);
    }
    [Fact]
    public void Emitter_GlobalScope_EmptyScriptRelease()
    {
        const string code = "";
        const string il = @"
            IL0000: ldsfld System.Object Program::<result>
            IL0005: ret
";
        code.AssertIl("<eval>", il, script: true);
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
            IL000E: dup
            IL000F: stsfld System.Int32 Program::a
            IL0014: pop
            IL0015: ldsfld System.Int32 Program::a
            IL001A: ldc.i4.2
            IL001B: cgt
            IL001D: brfalse.s IL_0021: ldsfld System.Int32 Program::a
            IL001F: br.s IL_0037: ret
            IL0021: ldsfld System.Int32 Program::a
            IL0026: box System.Int32
            IL002B: call System.String System.Convert::ToString(System.Object)
            IL0030: call System.Void System.Console::WriteLine(System.Object)
            IL0035: br.s IL_0037: ret
            IL0037: ret
";
        code.AssertIlAndSymbols("main", il);
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
            IL000D: dup
            IL000E: stsfld System.Int32 Program::a
            IL0013: pop
            IL0014: ldsfld System.Int32 Program::a
            IL0019: ldc.i4.2
            IL001A: cgt
            IL001C: brfalse.s IL_001f: ldsfld System.Int32 Program::a
            IL001E: ret
            IL001F: ldsfld System.Int32 Program::a
            IL0024: box System.Int32
            IL0029: call System.String System.Convert::ToString(System.Object)
            IL002E: call System.Void System.Console::WriteLine(System.Object)
            IL0033: ret";
        code.AssertIl("main", il);
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
            IL0019: dup
            IL001A: stsfld System.Object Program::<result>
            IL001F: pop
            IL0020: ldsfld System.Int32 Program::a
            IL0025: ldc.i4.2
            IL0026: cgt
            IL0028: brfalse.s IL_0036: ldsfld System.Int32 Program::a
            IL002A: ldsfld System.Int32 Program::a
            IL002F: box System.Int32
            IL0034: br.s IL_0051: ret
            IL0036: ldsfld System.Int32 Program::a
            IL003B: box System.Int32
            IL0040: call System.String System.Convert::ToString(System.Object)
            IL0045: call System.Void System.Console::WriteLine(System.Object)
            IL004A: ldsfld System.Object Program::<result>
            IL004F: br.s IL_0051: ret
            IL0051: ret
";
        code.AssertIlAndSymbols("<eval>", il, script: true);
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
            IL0018: dup
            IL0019: stsfld System.Object Program::<result>
            IL001E: pop
            IL001F: ldsfld System.Int32 Program::a
            IL0024: ldc.i4.2
            IL0025: cgt
            IL0027: brfalse.s IL_0034: ldsfld System.Int32 Program::a
            IL0029: ldsfld System.Int32 Program::a
            IL002E: box System.Int32
            IL0033: ret
            IL0034: ldsfld System.Int32 Program::a
            IL0039: box System.Int32
            IL003E: call System.String System.Convert::ToString(System.Object)
            IL0043: call System.Void System.Console::WriteLine(System.Object)
            IL0048: ldsfld System.Object Program::<result>
            IL004D: ret";
        code.AssertIl("<eval>", il, script: true);
    }

}