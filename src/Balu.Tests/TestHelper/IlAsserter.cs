using System;
using Balu.Syntax;
using System.IO;
using System.Linq;
using Balu.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;
namespace Balu.Tests.TestHelper;

static class IlAsserter
{
    public static void AssertIl(this string code, string methodToAssert, string expectedIL, bool script = false) =>
        AssertEmitter(code, methodToAssert, expectedIL, script);
    public static void AssertSymbols(this string code, string methodToAssert, bool script = false) =>
        AssertEmitter(code, methodToAssert, null, script);
    static void AssertEmitter(string code, string methodToAssert, string? expectedIL, bool script)
    {
        var annotated = AnnotatedText.Parse(code);
        var tree = SyntaxTree.Parse(SourceText.From(annotated.Text, "IlAsserter.b"));
        var compilation = script
                              ? Compilation.CreateScript(null, tree)
                              : Compilation.Create(tree);

        var symbolStream = expectedIL is null ? new MemoryStream() : null;
        var outputStream = new MemoryStream();
        try
        {
            var diagnostics = compilation.Emit("Balu", ReferenceProvider.References, outputStream, symbolStream);
            Assert.Empty(diagnostics);

            outputStream.Seek(0, SeekOrigin.Begin);
            symbolStream?.Seek(0, SeekOrigin.Begin);

            var parameters = new ReaderParameters
                { ReadSymbols = true, SymbolReaderProvider = new PortablePdbReaderProvider(), SymbolStream = symbolStream };
            var assembly = symbolStream is null
                               ? AssemblyDefinition.ReadAssembly(outputStream)
                               : AssemblyDefinition.ReadAssembly(outputStream, parameters);
            var programType = assembly.MainModule.GetType("Program");
            var method = programType.Methods.Single(method => method.Name == methodToAssert);
            if (expectedIL is not null)
            {
                var il = string.Join(Environment.NewLine,
                                     method.Body.Instructions.Select(
                                         instruction =>
                                             $"IL{instruction.Offset:0000}: {instruction.OpCode}{(instruction.Operand is null ? "" : $" {instruction.Operand}")}"));
                var expected = string.Join(Environment.NewLine,
                                           expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
                Assert.Equal(expected, il);
                return;
            }

            var expectedSpans = annotated.Spans.OrderBy(span => span.Start)
                                         .ThenByDescending(span => span.Length)
                                         .Select(span => new TextLocation(tree.Text, span))
                                         .Select(location => (location.StartLine + 1, location.StartCharacter + 1, location.EndLine + 1,
                                                                 location.EndCharacter + 1));
            var actualSpans = method.DebugInformation.SequencePoints.OrderBy(sp => sp.StartLine).ThenBy(sp => sp.StartColumn).ThenByDescending(sp => sp.EndLine).ThenByDescending(sp => sp.EndColumn).Select(sp => (sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn));
            Assert.Equal(expectedSpans, actualSpans);
        }
        finally
        {
            outputStream.Dispose();
            symbolStream?.Dispose();
        }
    }
}
