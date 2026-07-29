using Balu.Syntax;
using Balu.Tests.TestHelper;
using Balu.Diagnostics;
using Balu.Text;
using Balu.Visualization;
using System.IO;
using System.Linq;
using Xunit;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"normal string\"", "normal string")]
    [InlineData("\"Escaped\\\"String\"", "Escaped\"String")]
    public void Lexer_String(string input, string result)
    {
        var tokens = SyntaxTree.ParseTokens(input);
        var token = Assert.Single(tokens);
        Assert.Equal(SyntaxKind.StringToken, token.Kind);
        Assert.Equal(result, token.Value);
        Assert.Equal(input, token.Text);
    }
    [Fact]
    public void Lexer_String_UnescapesAllRequired()
    {
        var escaped = SyntaxFacts.EscapedToUnescapedCharacter.Keys.ToArray();
        var code = "\"\\" + string.Join("\\", escaped) + "\"";
        var expected = string.Join(string.Empty, escaped.Select(esc => SyntaxFacts.EscapedToUnescapedCharacter[esc]));
        var tokens = SyntaxTree.ParseTokens(code).ToArray();
        var token = Assert.Single(tokens);
        Assert.Equal(expected, token.Value);
        Assert.Equal(code, token.Text);
    }
    [Fact]
    public void Lexer_String_Reports_InvalidEscapeSequence()
    {
        "\"test\\[u]yeah\"".AssertLexerDiagnostics("BL0003: Invalid escape sequence 'u'.");
    }
    [Fact]
    public void Lexer_String_Reports_UnterminatedString()
    {
        "var x = [\"test]".AssertLexerDiagnostics("BL0004: String literal not terminated.");
    }
    [Fact]
    public void Lexer_String_Reports_UnterminatedStringForMultiline()
    {
        const string input = @"
            {
                var x = [""test       ]
                var z = 12
            }";
        input.AssertLexerDiagnostics("BL0004: String literal not terminated.");
    }
    [Fact]
    public void Lexer_String_AllowsEmbeddedNullCharacter()
    {
        const string input = "\"a\0b\"";

        var token = Assert.Single(SyntaxTree.ParseTokens(input, out var diagnostics));

        Assert.Empty(diagnostics);
        Assert.Equal(SyntaxKind.StringToken, token.Kind);
        Assert.Equal("a\0b", token.Value);
        Assert.Equal(input, token.Text);
    }
    [Fact]
    public void Lexer_String_TrailingBackslashProducesValidDiagnostics()
    {
        const string input = "\"abc\\";

        SyntaxTree.ParseTokens(input, out var diagnostics);

        Assert.Equal(2, diagnostics.Length);
        var invalidEscape = Assert.Single(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.InvalidEscapeSequence);
        Assert.Equal(new TextSpan(input.Length - 1, 1), invalidEscape.Location.Span);
        Assert.Equal("Invalid escape sequence '\\'.", invalidEscape.Message);
        var unterminated = Assert.Single(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.UnterminatedString);
        Assert.Equal(new TextSpan(0, input.Length), unterminated.Location.Span);
        Assert.All(diagnostics, diagnostic => Assert.InRange(diagnostic.Location.Span.End, diagnostic.Location.Span.Start, input.Length));

        using var writer = new StringWriter();
        writer.WriteDiagnostics(diagnostics);
        Assert.NotEmpty(writer.ToString());
    }
}
