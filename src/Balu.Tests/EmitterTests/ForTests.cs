using Balu.Tests.TestHelper;
using Xunit;
using Xunit.Abstractions;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    readonly ITestOutputHelper? output;
    public EmitterTests(ITestOutputHelper? output)
    {
        this.output = output;
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
                IL0006: ldloc.0
                IL0007: ldc.i4.s 10
                IL0009: cgt
                IL000B: ldc.i4.0
                IL000C: ceq
                IL000E: brfalse.s IL_002e: br.s IL_0030
                IL0010: nop
                IL0011: ldloc.0
                IL0012: ldc.i4.3
                IL0013: cgt
                IL0015: brfalse.s IL_0019: ldloc.0
                IL0017: br.s IL_002e: br.s IL_0030
                IL0019: ldloc.0
                IL001A: ldc.i4.5
                IL001B: cgt
                IL001D: brfalse.s IL_0021: ldc.i4.1
                IL001F: br.s IL_0026: ldloc.0
                IL0021: ldc.i4.1
                IL0022: dup
                IL0023: stloc.0
                IL0024: pop
                IL0025: nop
                IL0026: ldloc.0
                IL0027: ldc.i4.1
                IL0028: add
                IL0029: dup
                IL002A: stloc.0
                IL002B: pop
                IL002C: br.s IL_0006: ldloc.0
                IL002E: br.s IL_0030: ret
                IL0030: ret    
";
        code.AssertIlAndSymbols("test", il, output: output);
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
            IL0005: ldloc.0
            IL0006: ldc.i4.s 10
            IL0008: cgt
            IL000A: ldc.i4.0
            IL000B: ceq
            IL000D: brfalse.s IL_002b: ret
            IL000F: ldloc.0
            IL0010: ldc.i4.3
            IL0011: cgt
            IL0013: brfalse.s IL_0017: ldloc.0
            IL0015: br.s IL_002b: ret
            IL0017: ldloc.0
            IL0018: ldc.i4.5
            IL0019: cgt
            IL001B: brfalse.s IL_001f: ldc.i4.1
            IL001D: br.s IL_0023: ldloc.0
            IL001F: ldc.i4.1
            IL0020: dup
            IL0021: stloc.0
            IL0022: pop
            IL0023: ldloc.0
            IL0024: ldc.i4.1
            IL0025: add
            IL0026: dup
            IL0027: stloc.0
            IL0028: pop
            IL0029: br.s IL_0005: ldloc.0
            IL002B: ret
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
            IL0006: ldloc.0
            IL0007: ldc.i4.s 10
            IL0009: cgt
            IL000B: ldc.i4.0
            IL000C: ceq
            IL000E: brfalse.s IL_0022: br.s IL_0024
            IL0010: ldstr
            IL0015: call System.Void System.Console::WriteLine(System.Object)
            IL001A: ldloc.0
            IL001B: ldc.i4.1
            IL001C: add
            IL001D: dup
            IL001E: stloc.0
            IL001F: pop
            IL0020: br.s IL_0006: ldloc.0
            IL0022: br.s IL_0024: ret
            IL0024: ret
";
        code.AssertIlAndSymbols("test", il, output: output);
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
            IL0005: ldloc.0
            IL0006: ldc.i4.s 10
            IL0008: cgt
            IL000A: ldc.i4.0
            IL000B: ceq
            IL000D: brfalse.s IL_0021: ret
            IL000F: ldstr
            IL0014: call System.Void System.Console::WriteLine(System.Object)
            IL0019: ldloc.0
            IL001A: ldc.i4.1
            IL001B: add
            IL001C: dup
            IL001D: stloc.0
            IL001E: pop
            IL001F: br.s IL_0005: ldloc.0
            IL0021: ret
";
        code.AssertIl("test", il, output: output);
    }
}