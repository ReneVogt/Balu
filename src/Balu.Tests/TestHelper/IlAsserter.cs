using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Balu.Syntax;
using System.IO;
using System.Linq;
using Balu.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;
using Xunit.Abstractions;

namespace Balu.Tests.TestHelper;

static class IlAsserter
{
    public static void AssertIl(this string code, string methodToAssert, string expectedIL, bool script = false, bool debug = false, ITestOutputHelper? output = null)
    {
        var expected = string.Join(Environment.NewLine,
                                   expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
        var tree = SyntaxTree.Parse(SourceText.From(code, "IlAsserter.b"));
        var (il, _, _) = ExecuteEmitter(tree, methodToAssert, script, debug, output);
        Assert.Equal(expected, il);
    }
    public static void AssertSymbols(this string code, string methodToAssert, IEnumerable<int> sequencePointOffsets, string? scopes = null, bool script = false)
    {
        var annotated = AnnotatedText.Parse(code);
        var tree = SyntaxTree.Parse(SourceText.From(annotated.Text, "IlAsserter.b"));

        var expectedSymbols = string.Join(Environment.NewLine,
                                        annotated.Spans.OrderBy(span => span.Start)
                                                 .ThenByDescending(span => span.Length)
                                                 .Zip(sequencePointOffsets, (span, offset) => ToSymbolString(offset, new (tree.Text, span))));
        
        var (_, symbols, actualScopes) = ExecuteEmitter(tree, methodToAssert, script, true, null);
        Assert.Equal(expectedSymbols, symbols);
        if (scopes is not null) Assert.Equal(AnnotatedText.Parse(scopes).Text, actualScopes);
    }
    public static void AssertIlAndSymbols(this string code, string methodToAssert, string expectedIL, IEnumerable<int> sequencePointOffsets, string? scopes = null, bool script = false, ITestOutputHelper? output = null)
    {
        var expected = string.Join(Environment.NewLine,
                                   expectedIL.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
        var annotated = AnnotatedText.Parse(code);
        var tree = SyntaxTree.Parse(SourceText.From(annotated.Text, "IlAsserter.b"));
        var expectedSymbols = string.Join(Environment.NewLine,
                                          annotated.Spans.OrderBy(span => span.Start)
                                                   .ThenByDescending(span => span.Length)
                                                   .Zip(sequencePointOffsets,
                                                    (span, offset) => ToSymbolString(offset, new (tree.Text, span))));

        var (il, symbols, actualScopes) = ExecuteEmitter(tree, methodToAssert, script, true, output);
        Assert.Equal(expected, il);
        Assert.Equal(expectedSymbols, symbols);
        if (scopes is not null) Assert.Equal(AnnotatedText.Parse(scopes).Text.TrimEnd(), actualScopes);
    }
    static (string il, string symbols, string locals) ExecuteEmitter(SyntaxTree syntaxTree, string methodToAssert, bool script, bool debug, ITestOutputHelper? output)
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
                                         $"IL{instruction.Offset:X04}: {instruction.OpCode}{(string.IsNullOrWhiteSpace(instruction.Operand?.ToString()) ? "" : $" {instruction.Operand}")}"));
            var orderedSequencePoints = method.DebugInformation.SequencePoints
                                              .OrderBy(sp => sp.StartLine)
                                              .ThenBy(sp => sp.StartColumn)
                                              .ThenByDescending(sp => sp.EndLine)
                                              .ThenByDescending(sp => sp.EndColumn)
                                              .ToArray();
            var symbols = string.Join(Environment.NewLine,
                                      orderedSequencePoints.Select(ToSymbolString));
            var scopeWriter = new IndentedTextWriter(new StringWriter(), " ");
            BuildScopes(scopeWriter, method.DebugInformation.Scope);
            var locals = scopeWriter.InnerWriter.ToString()!.TrimEnd();
            output?.WriteLine("IL:");
            output?.WriteLine(il);
            output?.WriteLine("Sequence points:");
            output?.WriteLine(symbols);
            output?.WriteLine("Locals:");
            output?.WriteLine(locals);
            return (il, symbols, locals);
        }
        finally
        {
            outputStream.Dispose();
            symbolStream?.Dispose();
        }

        static void BuildScopes(IndentedTextWriter writer, ScopeDebugInformation? scope)
        {
            if (scope is null) return;
            writer.WriteLine($"<BEGIN {scope.Start.Offset:X04}>");
            foreach(var variable in scope.Variables.OrderBy(v => v.Name))
                writer.WriteLine(variable.Name);
            writer.Indent++;
            foreach (var subScope in scope.Scopes)
                BuildScopes(writer, subScope);
            writer.Indent--;
            writer.WriteLine($"<END {scope.End.Offset:X04}>");
        }
    }

    static string ToSymbolString(SequencePoint sequencePoint) =>
        ToSymbolString(sequencePoint.Offset, sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
    static string ToSymbolString(int offset, TextLocation location) =>
        ToSymbolString(offset, location.StartLine + 1, location.StartCharacter + 1, location.EndLine + 1, location.EndCharacter + 1);
    static string ToSymbolString(int offset, int startLine, int startColumn, int endLine, int endColumn) =>
        $"{offset:X04}: ({startLine}, {startColumn}) - ({endLine}, {endColumn})";
}
