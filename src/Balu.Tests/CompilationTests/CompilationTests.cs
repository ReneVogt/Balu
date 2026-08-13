using System;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.CompilationTests;

public sealed class CompilationTests
{
    [Fact]
    public void Compilation_ThrowsIfCombinedWithFailed()
    {
        var compilation = Compilation.CreateScript(null, SyntaxTree.Parse("function a() { var x = y }"));
        Assert.True(compilation.Diagnostics.HasErrors());
        Assert.Throws<ArgumentException>("previous", () => Compilation.CreateScript(compilation, SyntaxTree.Parse("function a() { var x = y }")));
    }

    [Fact]
    public void WriteControlFlowGraph_ThrowsForNullFunction()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));

        Assert.Throws<ArgumentNullException>("function", () => compilation.WriteControlFlowGraph(TextWriter.Null, null!));
    }

    [Fact]
    public void WriteControlFlowGraph_ThrowsForNullWriter()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        var function = GetFunction(compilation, "main");

        Assert.Throws<ArgumentNullException>("writer", () => compilation.WriteControlFlowGraph(null!, function));
    }

    [Fact]
    public void WriteControlFlowGraph_ThrowsForBuiltInFunction()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        var function = GetFunction(compilation, "print");

        Assert.Throws<ArgumentException>("function", () => compilation.WriteControlFlowGraph(TextWriter.Null, function));
    }

    [Fact]
    public void WriteControlFlowGraph_ThrowsForFunctionFromAnotherCompilation()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        var otherCompilation = Compilation.Create(SyntaxTree.Parse("function other() {}"));
        var function = GetFunction(otherCompilation, "other");

        Assert.Throws<ArgumentException>("function", () => compilation.WriteControlFlowGraph(TextWriter.Null, function));
    }

    [Fact]
    public void WriteControlFlowGraph_WritesGraphForUserFunction()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        var function = GetFunction(compilation, "main");
        using var writer = new StringWriter();

        compilation.WriteControlFlowGraph(writer, function);

        Assert.StartsWith("digraph G {", writer.ToString());
    }

    static FunctionSymbol GetFunction(Compilation compilation, string name) =>
        compilation.VisibleSymbols.OfType<FunctionSymbol>().Single(function => function.Name == name);
}
