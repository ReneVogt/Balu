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
    public static void AssertIl(this string code, string methodToAssert, string expectedIL, bool script = false, bool debug = false)
    {
        var expected = string.Join(Environment.NewLine,
                                   expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
        var tree = SyntaxTree.Parse(SourceText.From(code, "IlAsserter.b"));
        var (il, _) = ExecuteEmitter(tree, methodToAssert, script, debug);
        Assert.Equal(expected, il);
    }
    public static void AssertSymbols(this string code, string methodToAssert, bool script = false)
    {
        var annotated = AnnotatedText.Parse(code);
        var tree = SyntaxTree.Parse(SourceText.From(annotated.Text, "IlAsserter.b"));

        var expectedSymbols = string.Join(Environment.NewLine,
                                        annotated.Spans.OrderBy(span => span.Start)
                                                 .ThenByDescending(span => span.Length)
                                                 .Select(span => ToSymbolString(new TextLocation(tree.Text, span))));
        
        var (_, symbols) = ExecuteEmitter(tree, methodToAssert, script, true);
        Assert.Equal(expectedSymbols, symbols);
    }
    public static void AssertIlAndSymbols(this string code, string methodToAssert, string expectedIL, bool script = false)
    {
        var expected = string.Join(Environment.NewLine,
                                   expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
        var annotated = AnnotatedText.Parse(code);
        var tree = SyntaxTree.Parse(SourceText.From(annotated.Text, "IlAsserter.b"));
        var expectedSymbols = string.Join(Environment.NewLine,
                                          annotated.Spans.OrderBy(span => span.Start)
                                                   .ThenByDescending(span => span.Length)
                                                   .Select(span => ToSymbolString(new TextLocation(tree.Text, span))));

        var (il, symbols) = ExecuteEmitter(tree, methodToAssert, script, true);
        Assert.Equal(expected, il);
        Assert.Equal(expectedSymbols, symbols);

    }
    static (string il, string symbols) ExecuteEmitter(SyntaxTree syntaxTree, string methodToAssert, bool script, bool debug)
    {
        var compilation = script
                              ? Compilation.CreateScript(null, syntaxTree)
                              : Compilation.Create(syntaxTree);

        var symbolStream = debug ? new MemoryStream() : null;
        var outputStream = new MemoryStream();
        try
        {
            var diagnostics = compilation.Emit("Balu", ReferenceProvider.References, outputStream, symbolStream);
            Assert.Empty(diagnostics);

            outputStream.Seek(0, SeekOrigin.Begin);
            symbolStream?.Seek(0, SeekOrigin.Begin);

            var parameters = new ReaderParameters
                { ReadSymbols = true, SymbolReaderProvider = new PortablePdbReaderProvider(), SymbolStream = symbolStream };
            var assembly = debug
                               ? AssemblyDefinition.ReadAssembly(outputStream, parameters)
                               : AssemblyDefinition.ReadAssembly(outputStream);
            var programType = assembly.MainModule.GetType("Program");
            var method = programType.Methods.Single(method => method.Name == methodToAssert);
            var il = string.Join(Environment.NewLine,
                                 method.Body.Instructions.Select(
                                     instruction =>
                                         $"IL{instruction.Offset:X04}: {instruction.OpCode}{(instruction.Operand is null ? "" : $" {instruction.Operand}")}"));
            var symbols = string.Join(Environment.NewLine,
                                      method.DebugInformation.SequencePoints
                                            .OrderBy(sp => sp.StartLine)
                                            .ThenBy(sp => sp.StartColumn)
                                            .ThenByDescending(sp => sp.EndLine)
                                            .ThenByDescending(sp => sp.EndColumn)
                                            .Select(ToSymbolString));
            return (il, symbols);
        }
        finally
        {
            outputStream.Dispose();
            symbolStream?.Dispose();
        }
    }

    static string ToSymbolString(SequencePoint sequencePoint) =>
        ToSymbolString(sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
    static string ToSymbolString(TextLocation location) =>
        ToSymbolString(location.StartLine + 1, location.StartCharacter + 1, location.EndLine + 1, location.EndCharacter + 1);
    static string ToSymbolString(int startLine, int startColumn, int endLine, int endColumn) =>
        $"({startLine}, {startColumn}) - ({endLine}, {endColumn})";
}
