using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_LocalScopes_SimpleFunction()
    {
        const string code = @"
        function test()
        [{]
            [var i=1]
            [i = 0]
        [}]
        test()                          
";
        const string il = @"
            IL0000: nop
            IL0001: ldc.i4.1
            IL0002: stloc.0
            IL0003: ldc.i4.0
            IL0004: stloc.0
            IL0005: nop
            IL0006: ret
";
        const string scopes = @"
            <BEGIN 0000>
             <BEGIN 0002>
             i
             <END 0006>
            <END 0006>
";
        var offsets = new[] { 0, 1, 3, 5 };

        code.AssertIlAndSymbols("test", il, offsets, scopes, output: output);
    }
    [Fact]
    public void Emitter_LocalScopes_CorrectScopes()
    {
        const string code = @"
        function test(argument:int)
        [{]
            for [loopVariable = 1] [[to] [10]]
            [{]
                [var x = 1]
                [if x > loopVariable]
                [{]
                    [var y = x]
                [}]
                [var z = 2 * x]
            [}]
            [var ende = true]
            [if (ende)]
                [var schluss = false]
        [}]
        test(12)                          
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
            IL0010: brtrue.s IL_0035: ldc.i4.1
            IL0012: ldloc.0
            IL0013: ldc.i4.1
            IL0014: add
            IL0015: stloc.0
            IL0016: ldloc.0
            IL0017: ldc.i4.s 10
            IL0019: cgt
            IL001B: ldc.i4.0
            IL001C: ceq
            IL001E: brfalse.s IL_0035: ldc.i4.1
            IL0020: nop
            IL0021: ldc.i4.1
            IL0022: stloc.2
            IL0023: ldloc.2
            IL0024: ldloc.0
            IL0025: cgt
            IL0027: brfalse.s IL_002d: ldc.i4.2
            IL0029: nop
            IL002A: ldloc.2
            IL002B: stloc.3
            IL002C: nop
            IL002D: ldc.i4.2
            IL002E: ldloc.2
            IL002F: mul
            IL0030: stloc.s V_4
            IL0032: nop
            IL0033: br.s IL_0008: ldc.i4.s 10
            IL0035: ldc.i4.1
            IL0036: stloc.s V_5
            IL0038: ldloc.s V_5
            IL003A: brfalse.s IL_003f: nop
            IL003C: ldc.i4.0
            IL003D: stloc.s V_6
            IL003F: nop
            IL0040: ret
";
        const string scopes = @"
            <BEGIN 0000>
             <BEGIN 0001>
              <BEGIN 0002>
              loopVariable
               <BEGIN 0005>
                <BEGIN 0020>
                 <BEGIN 0022>
                 x
                  <BEGIN 0029>
                   <BEGIN 002B>
                   y
                   <END 002D>
                  <END 002D>
                  <BEGIN 0030>
                  z
                  <END 0033>
                 <END 0033>
                <END 0033>
               <END 0035>
              <END 0035>
             <END 0035>
             <BEGIN 0036>
             ende
              <BEGIN 003C>
               <BEGIN 003D>
               schluss
               <END 003F>
              <END 003F>
             <END 0040>
            <END 0040>
";
        var offsets = new[] { 0, 1, 3, 0x12, 0x16, 0x20, 0x21, 0x23, 0x29, 0x2A, 0x2C, 0x2D, 0x32, 0x35, 0x38, 0x3C, 0x3F };

        code.AssertIlAndSymbols("test", il, offsets, scopes, output: output);

    }
}
