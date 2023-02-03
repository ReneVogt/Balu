using Balu.Tests.TestHelper;
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
        var offsets = new[] { 0, 1, 3, 5};

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
            IL0006: br.s IL_000c: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: stloc.0
            IL000C: ldloc.0
            IL000D: ldc.i4.s 10
            IL000F: cgt
            IL0011: ldc.i4.0
            IL0012: ceq
            IL0014: brfalse.s IL_002b: ldc.i4.1
            IL0016: nop
            IL0017: ldc.i4.1
            IL0018: stloc.2
            IL0019: ldloc.2
            IL001A: ldloc.0
            IL001B: cgt
            IL001D: brfalse.s IL_0023: ldc.i4.2
            IL001F: nop
            IL0020: ldloc.2
            IL0021: stloc.3
            IL0022: nop
            IL0023: ldc.i4.2
            IL0024: ldloc.2
            IL0025: mul
            IL0026: stloc.s V_4
            IL0028: nop
            IL0029: br.s IL_0008: ldloc.0
            IL002B: ldc.i4.1
            IL002C: stloc.s V_5
            IL002E: ldloc.s V_5
            IL0030: brfalse.s IL_0035: nop
            IL0032: ldc.i4.0
            IL0033: stloc.s V_6
            IL0035: nop
            IL0036: ret
";
        const string scopes = @"
            <BEGIN 0000>
             <BEGIN 0001>
              <BEGIN 0002>
              loopVariable
               <BEGIN 0005>
                <BEGIN 0016>
                 <BEGIN 0018>
                 x
                  <BEGIN 001F>
                   <BEGIN 0021>
                   y
                   <END 0023>
                  <END 0023>
                  <BEGIN 0026>
                  z
                  <END 0029>
                 <END 0029>
                <END 0029>
               <END 002B>
              <END 002B>
             <END 002B>
             <BEGIN 002C>
             ende
              <BEGIN 0032>
               <BEGIN 0033>
               schluss
               <END 0035>
              <END 0035>
             <END 0036>
            <END 0036>
";
        var offsets = new[] { 0, 1, 3, 8, 0xC, 0x16, 0x17, 0x19, 0x1F, 0x20, 0x22, 0x23, 0x28, 0x2B, 0x2E, 0x32, 0x35 };

        code.AssertIlAndSymbols("test", il, offsets, scopes, output: output);

    }
}