using Balu.Syntax;
using Balu.Tests.TestHelper;
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
        "\"test\\[u]yeah\"".AssertLexerDiagnostics("Invalid escape sequence 'u'.");
    }
    [Fact]
    public void Lexer_String_Reports_UnterminatedString()
    {
        "var x = [\"test]".AssertLexerDiagnostics("String literal not terminated.");
    }
    [Fact]
    public void Lexer_String_Reports_UnterminatedStringForMultiline()
    {
        const string input = @"
            {
                var x = [""test       ]
                var z = 12
            }";
        input.AssertLexerDiagnostics("String literal not terminated.");
    }
}
