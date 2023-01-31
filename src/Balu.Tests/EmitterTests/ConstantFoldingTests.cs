using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_ConstantFolding_IfTrueDebug()
    {
        const string code = @"
            function call() {}
            function test() [{]
                [let x = 4]
                [let y = 5]
                if (x < y)
                [{]
                    [call()]
                [}]
                else {}
            [}]
            test()
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.4
            IL0002: stloc.0
            IL0003: ldc.i4.5
            IL0004: stloc.1
            IL0005: nop
            IL0006: call System.Void Program::call()
            IL000B: nop
            IL000C: nop
            IL000D: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 0xB, 0xC };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
}