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
            IL0004: dup
            IL0005: stloc.0
            IL0006: pop
            IL0007: nop
            IL0008: ret
";
        const string scopes = @"
            <BEGIN 0000>
             <BEGIN 0002>
             i
             <END 0008>
            <END 0008>
";
        var offsets = new[] { 0, 1, 3, 7};

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
            IL0006: br.s IL_000e: ldloc.0
            IL0008: ldloc.0
            IL0009: ldc.i4.1
            IL000A: add
            IL000B: dup
            IL000C: stloc.0
            IL000D: pop
            IL000E: ldloc.0
            IL000F: ldc.i4.s 10
            IL0011: cgt
            IL0013: ldc.i4.0
            IL0014: ceq
            IL0016: brfalse.s IL_002d: ldc.i4.1
            IL0018: nop
            IL0019: ldc.i4.1
            IL001A: stloc.2
            IL001B: ldloc.2
            IL001C: ldloc.0
            IL001D: cgt
            IL001F: brfalse.s IL_0025: ldc.i4.2
            IL0021: nop
            IL0022: ldloc.2
            IL0023: stloc.3
            IL0024: nop
            IL0025: ldc.i4.2
            IL0026: ldloc.2
            IL0027: mul
            IL0028: stloc.s V_4
            IL002A: nop
            IL002B: br.s IL_0008: ldloc.0
            IL002D: ldc.i4.1
            IL002E: stloc.s V_5
            IL0030: ldloc.s V_5
            IL0032: brfalse.s IL_0037: nop
            IL0034: ldc.i4.0
            IL0035: stloc.s V_6
            IL0037: nop
            IL0038: ret
";
        const string scopes = @"
            <BEGIN 0000>
             <BEGIN 0001>
              <BEGIN 0002>
              loopVariable
               <BEGIN 0005>
                <BEGIN 0018>
                 <BEGIN 001A>
                 x
                  <BEGIN 0021>
                   <BEGIN 0023>
                   y
                   <END 0025>
                  <END 0025>
                  <BEGIN 0028>
                  z
                  <END 002B>
                 <END 002B>
                <END 002B>
               <END 002D>
              <END 002D>
             <END 002D>
             <BEGIN 002E>
             ende
              <BEGIN 0034>
               <BEGIN 0035>
               schluss
               <END 0037>
              <END 0037>
             <END 0038>
            <END 0038>
";
        var offsets = new[] { 0, 1, 3, 8, 0xE, 0x18, 0x19, 0x1B, 0x21, 0x22, 0x24, 0x25, 0x2A, 0x2D, 0x30, 0x34, 0x37 };

        code.AssertIlAndSymbols("test", il, offsets, scopes, output: output);

    }
}