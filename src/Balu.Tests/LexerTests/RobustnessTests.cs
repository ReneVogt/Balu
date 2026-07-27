using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Text;
using Balu.Visualization;
using Xunit;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
    [Fact]
    public void Lexer_DiagnosticRenderingRecognizesLoneCarriageReturnAsLineEnding()
    {
        SyntaxTree.ParseTokens("a\r?", out var diagnostics);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(1, diagnostic.Location.StartLine);
        Assert.Equal(0, diagnostic.Location.StartCharacter);
        using var writer = new StringWriter();
        writer.WriteDiagnostics(diagnostics);
        Assert.Contains("(2,1): error", writer.ToString());
    }

    [Fact]
    public void Lexer_GeneratedInputsProduceValidDiagnostics()
    {
        ReadOnlySpan<char> alphabet = ['\0', '\r', '\n', '"', '\\', '/', '*', 'a', '1', ' ', '{', '}'];
        var random = new Random(87);

        for (int test = 0; test < 1_000; test++)
        {
            var builder = new StringBuilder();
            var length = random.Next(33);
            for (int i = 0; i < length; i++)
                builder.Append(alphabet[random.Next(alphabet.Length)]);

            var source = SourceText.From(builder.ToString());
            var tokens = SyntaxTree.ParseTokens(source, out var lexerDiagnostics);
            var tree = SyntaxTree.Parse(source);

            Assert.All(tokens, token => AssertValidSpan(source, token.Span));
            Assert.Equal(source.Length, tree.Root.EndOfFileToken.Span.Start);
            AssertDiagnostics(source, lexerDiagnostics);
            AssertDiagnostics(source, tree.Diagnostics);
        }
    }

    static void AssertDiagnostics(SourceText source, IEnumerable<Diagnostic> diagnostics)
    {
        var diagnosticArray = diagnostics is Diagnostic[] array ? array : [.. diagnostics];
        foreach (var diagnostic in diagnosticArray)
        {
            Assert.Same(source, diagnostic.Location.Text);
            AssertValidSpan(source, diagnostic.Location.Span);
            _ = diagnostic.Location.StartLine;
            _ = diagnostic.Location.EndLine;
            _ = diagnostic.Location.StartCharacter;
            _ = diagnostic.Location.EndCharacter;
            _ = source.ToString(diagnostic.Location.Span);
        }

        using var writer = new StringWriter();
        writer.WriteDiagnostics(diagnosticArray);
    }

    static void AssertValidSpan(SourceText source, TextSpan span)
    {
        Assert.InRange(span.Start, 0, source.Length);
        Assert.InRange(span.End, span.Start, source.Length);
    }
}
