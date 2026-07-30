using TestHelpers;
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
                [if (x < y)]
                    [call()]
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
            IL000C: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 0xB };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfTrueRelease()
    {
        const string code = @"
            function call() {}
            function test() {
                let x = 4
                let y = 5
                if (x < y)
                    call()
            }
            test()
";
        const string il = @"
        IL0000: ldc.i4.4
        IL0001: stloc.0
        IL0002: ldc.i4.5
        IL0003: stloc.1
        IL0004: call System.Void Program::call()
        IL0009: ret
";
        code.AssertIl("test", il, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfTrueOptimizedWithSymbols()
    {
        const string code = @"
            function call() {}
            function test() {
                [let x = 4]
                [let y = 5]
                if (x < y)
                    [call()]
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.4
            IL0001: stloc.0
            IL0002: ldc.i4.5
            IL0003: stloc.1
            IL0004: call System.Void Program::call()
            IL0009: ret
";
        var offsets = new[] { 0, 2, 4 };
        code.AssertIlAndSymbols("test", il, offsets, debugFriendly: false, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfFalseDebug()
    {
        const string code = @"
            function call() {}
            function test() [{]
                [let x = 4]
                [let y = 5]
                [if (x > y)]
                {
                    call()
                }
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
            IL0006: nop
            IL0007: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfFalseRelease()
    {
        const string code = @"
            function call() {}
            function test() {
                let x = 4
                let y = 5
                if (x > y)
                {
                    call()
                }
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.4
            IL0001: stloc.0
            IL0002: ldc.i4.5
            IL0003: stloc.1
            IL0004: ret
";
        code.AssertIl("test", il, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfElseTrueDebug()
    {
        const string code = @"
            function call() {}
            function test() [{]
                [let x = 4]
                [let y = 5]
                [if (x < y)]
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
            IL0006: nop
            IL0007: call System.Void Program::call()
            IL000C: nop
            IL000D: nop
            IL000E: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 7, 0xC, 0xD };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfElseTrueRelease()
    {
        const string code = @"
            function call() {}
            function test() {
                let x = 4
                let y = 5
                if (x < y)
                {
                    call()
                }
                else {}
            }
            test()
";
        const string il = @"
        IL0000: ldc.i4.4
        IL0001: stloc.0
        IL0002: ldc.i4.5
        IL0003: stloc.1
        IL0004: call System.Void Program::call()
        IL0009: ret
";
        code.AssertIl("test", il, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfElseFalseDebug()
    {
        const string code = @"
            function call() {}
            function test() [{]
                [let x = 4]
                [let y = 5]
                [if (x > y)]
                {
                    call()
                }
                else [{][}]
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
            IL0006: nop
            IL0007: nop
            IL0008: nop
            IL0009: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 7, 8 };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_IfElseFalseRelease()
    {
        const string code = @"
            function call() {}
            function test() {
                let x = 4
                let y = 5
                if (x > y)
                {
                    call()
                }
                else {}
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.4
            IL0001: stloc.0
            IL0002: ldc.i4.5
            IL0003: stloc.1
            IL0004: ret
";
        code.AssertIl("test", il, output: output);
    }

    [Fact]
    public void Emitter_ConstantFolding_WhileTrueDebug()
    {
        const string code = @"
            function call() {}
            function call2() {}
            function test() [{]
                [let x = true]
                [let y = false]
                [while x || y]
                [{]
                    [call()]
                [}]
                call2()
            [}] 
            test()
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stloc.0
            IL0003: ldc.i4.0
            IL0004: stloc.1
            IL0005: nop
            IL0006: nop
            IL0007: call System.Void Program::call()
            IL000C: nop
            IL000D: br.s IL_0005: nop
            IL000F: nop
            IL0010: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 7, 0xC, 0xF };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_WhileTrueRelease()
    {
        const string code = @"
            function call() {}
            function call2() {}
            function test() {
                let x = true
                let y = true
                while x || y
                {
                    call()
                }

                call2()
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.1
            IL0001: stloc.0
            IL0002: ldc.i4.1
            IL0003: stloc.1
            IL0004: call System.Void Program::call()
            IL0009: br.s IL_0004: call System.Void Program::call()
            IL000B: ret
";
        code.AssertIl("test", il, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_WhileFalseDebug()
    {
        const string code = @"
            function call() {}
            function call2() {}
            function test() [{]
                [let x = false]
                [let y = true]
                [while x && y]
                {
                    call()
                }
                [call2()]
            [}]
            test()
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.0
            IL0002: stloc.0
            IL0003: ldc.i4.1
            IL0004: stloc.1
            IL0005: nop
            IL0006: call System.Void Program::call2()
            IL000B: nop
            IL000C: ret
";
        var offsets = new[] { 0, 1, 3, 5, 6, 0xB };
        code.AssertIlAndSymbols("test", il, offsets, output: output);
    }
    [Fact]
    public void Emitter_ConstantFolding_WhileFalseRelease()
    {
        const string code = @"
            function call() {}
            function call2() {}
            function test() {
                let x = false
                let y = true
                while x && y
                {
                    call()
                }
                call2()
            }
            test()
";
        const string il = @"
            IL0000: ldc.i4.0
            IL0001: stloc.0
            IL0002: ldc.i4.1
            IL0003: stloc.1
            IL0004: call System.Void Program::call2()
            IL0009: ret
";
        code.AssertIl("test", il, output: output);
    }

}
