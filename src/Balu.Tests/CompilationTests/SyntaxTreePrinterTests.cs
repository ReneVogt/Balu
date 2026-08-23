using System;
using System.IO;
using System.Linq;
using Balu.Syntax;
using Balu.Visualization;
using Xunit;

namespace Balu.Tests.CompilationTests;

public sealed class SyntaxTreePrinterTests
{
    [Fact]
    public void SyntaxTreePrinter_IsNotASyntaxTreeVisitor() =>
        Assert.False(typeof(SyntaxTreeVisitor).IsAssignableFrom(typeof(SyntaxTreePrinter)));

    [Fact]
    public void SyntaxTreePrinter_PrintsTree()
    {
        var syntaxTree = SyntaxTree.Parse("1");
        using var writer = new StringWriter();

        SyntaxTreePrinter.Print(syntaxTree.Root, writer);

        var expected = string.Join(
            Environment.NewLine,
            "└──CompilationUnit(0, 1)",
            "   ├──GlobalStatement(0, 1)",
            "   │  └──ExpressionStatement(0, 1)",
            "   │     └──LiteralExpression(0, 1)",
            "   │        └──NumberToken(0, 1) \"1\" (1)",
            "   └──EndOfFileToken(1, 0) \"\0\" ()",
            string.Empty);
        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void SyntaxTreePrinter_PrintsDeepTree()
    {
        var source = string.Join(" + ", Enumerable.Repeat("1", 200));
        var syntaxTree = SyntaxTree.Parse(source);
        using var writer = new StringWriter();

        SyntaxTreePrinter.Print(syntaxTree.Root, writer);

        Assert.Contains("CompilationUnit", writer.ToString());
        Assert.Equal(200, writer.ToString().Split("NumberToken").Length - 1);
    }

    [Fact]
    public void SyntaxTreePrinter_PrintsTokenTriviaInOrder()
    {
        var syntaxTree = SyntaxTree.Parse(" 1 ");
        using var writer = new StringWriter();

        SyntaxTreePrinter.Print(syntaxTree.Root, writer);

        var lines = writer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        var leadingIndex = Array.FindIndex(lines, line => line.EndsWith("├──L: WhiteSpaceTrivia", StringComparison.Ordinal));
        var tokenIndex = Array.FindIndex(lines, line => line.Contains("├──NumberToken", StringComparison.Ordinal));
        var trailingIndex = Array.FindIndex(lines, line => line.EndsWith("└──T: WhiteSpaceTrivia", StringComparison.Ordinal));
        Assert.True(leadingIndex >= 0);
        Assert.True(leadingIndex < tokenIndex);
        Assert.True(tokenIndex < trailingIndex);
    }
}
