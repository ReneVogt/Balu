using System;
using Balu.Syntax;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;
namespace Balu.Tests.TestHelper;

static class IlAsserter
{
    internal static void AssertIl(this string code, string methodToAssert, string expectedIL, bool script = false, bool debug = false)
    {
        var compilation = script
                              ? Compilation.CreateScript(null, SyntaxTree.Parse(code))
                              : Compilation.Create(SyntaxTree.Parse(code));

        var symbolStream = debug ? new MemoryStream() : null;
        using var outputStream = new MemoryStream();
        using(symbolStream)
        {
            var diagnostics = compilation.Emit("Balu", ReferenceProvider.References, outputStream, symbolStream);
            Assert.Empty(diagnostics);
        }

        outputStream.Seek(0, SeekOrigin.Begin);
        var assembly = AssemblyDefinition.ReadAssembly(outputStream);
        var programType = assembly.MainModule.GetType("Program");
        var method = programType.Methods.Single(method => method.Name == methodToAssert);
        var il = string.Join(Environment.NewLine,
                             method.Body.Instructions.Select(
                                 instruction => $"{instruction.OpCode}{(instruction.Operand is null ? "" : $" {instruction.Operand}")}"));
        var expected = string.Join(Environment.NewLine, expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
        Assert.Equal(expected, il);
    }
}
