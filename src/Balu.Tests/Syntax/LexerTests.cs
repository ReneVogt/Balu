using Balu.Syntax;
using Balu.Tests.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Balu.Tests.Syntax;

public class LexerTests
{
    [Fact]
    public void Lexer_Tests_CoveringAllTokens()
    {
        var nonTestingKinds = new[] { SyntaxKind.EndOfFileToken, SyntaxKind.BadTokenTrivia };
        var allTokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                .Cast<SyntaxKind>()
                                .Where(kind => kind.IsToken())
                                .Except(nonTestingKinds);
        var testedTokens = ProvideSingleTokens().Select(x => (SyntaxKind)x[1]).Distinct();
        var untestedTokenKinds = allTokenKinds.Except(testedTokens).ToList();
        Assert.Empty(untestedTokenKinds);
    }
    [Fact]
    public void Lexer_Lexes_EmptyInput()
    {
        Assert.Empty(SyntaxTree.ParseTokens(string.Empty));
    }
    [Theory]
    [MemberData(nameof(ProvideSingleTokens))]
    public void Lexer_Lexes_Token(string text, SyntaxKind kind)
    {
        var parsedToken = Assert.Single(SyntaxTree.ParseTokens(text));
        Assert.Equal(kind, parsedToken.Kind);
        Assert.Equal(text, parsedToken.Text);
    }
    [Theory]
    [MemberData(nameof(ProvideTokenPairs))]
    public void Lexer_Lexes_TokenPairs(string text1, SyntaxKind kind1, string text2, SyntaxKind kind2)
    {
        var tokens = SyntaxTree.ParseTokens(text1 + text2);
        Assert.Equal(2, tokens.Length);
        Assert.Equal(kind1, tokens[0].Kind);
        Assert.Equal(text1, tokens[0].Text);
        Assert.Equal(kind2, tokens[1].Kind);
        Assert.Equal(text2, tokens[1].Text);
    }
    [Theory]
    [MemberData(nameof(ProvideSeparatedTokenPairs))]
    public void Lexer_Lexes_SeparatedTokenPairs(string text1, SyntaxKind kind1, string separatorText, SyntaxKind separatorKind, string text2, SyntaxKind kind2)
    {
        var tokens = SyntaxTree.ParseTokens(text1 + separatorText + text2);
        Assert.Equal(2, tokens.Length);
        Assert.Equal(kind1, tokens[0].Kind);
        Assert.Equal(text1, tokens[0].Text);
        Assert.Empty(tokens[0].LeadingTrivia);
        var actualSeparator = Assert.Single(tokens[0].TrailingTrivia);
        Assert.Equal(separatorKind, actualSeparator.Kind);
        Assert.Equal(separatorText, actualSeparator.Text);
        Assert.Equal(kind2, tokens[1].Kind);
        Assert.Equal(text2, tokens[1].Text);
        Assert.Empty(tokens[1].LeadingTrivia);
        Assert.Empty(tokens[1].TrailingTrivia);
    }

    [Theory]
    [InlineData("identifier")]
    [InlineData("_identifier")]
    [InlineData("identifier_")]
    [InlineData("identifier123c")]
    [InlineData("identifier123")]
    [InlineData("_id_123_ifier123")]
    [InlineData("_identifier123_")]
    public void Lexer_Identifier(string input)
    {
        var tokens = SyntaxTree.ParseTokens(input);
        var token = Assert.Single(tokens);
        Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
        Assert.Equal(input, token.Text);
    }


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
    
    public static IEnumerable<object[]> ProvideSingleTokens() => GetSingleTokens().Select(x => new object[] { x.text, x.kind });
    public static IEnumerable<(string text, SyntaxKind kind)> GetSingleTokens() =>
        Enum.GetValues(typeof(SyntaxKind))
            .Cast<SyntaxKind>()
            .Select(kind => (text: kind.GetText()!, kind))
            .Where(x => !string.IsNullOrEmpty(x.text))
            .Concat(
                new (string text, SyntaxKind kind)[]
                {
                    ("01", kind: SyntaxKind.NumberToken),
                    ("123", kind: SyntaxKind.NumberToken),
                    ("987654321", kind: SyntaxKind.NumberToken),
                    ("myNameIs", kind: SyntaxKind.IdentifierToken),
                    ("_my123", kind: SyntaxKind.IdentifierToken),
                    ("my_123_NameIs", kind: SyntaxKind.IdentifierToken),
                    ("x", kind: SyntaxKind.IdentifierToken),
                    ("true", kind: SyntaxKind.TrueKeyword),
                    ("false", kind: SyntaxKind.FalseKeyword),
                    ("\"Escaped\\\"String with even \\r and \\n, \\t and \\v\"", SyntaxKind.StringToken)
                });
    static IEnumerable<(string text, SyntaxKind kind)> GetSeparators() => new (string text, SyntaxKind kind)[]
    {
        (" ", kind: SyntaxKind.WhiteSpaceTrivia),
        ("  ", kind: SyntaxKind.WhiteSpaceTrivia),
        ("\r\n ", kind: SyntaxKind.WhiteSpaceTrivia),
        ("\t\v", kind: SyntaxKind.WhiteSpaceTrivia)
    };

    public static IEnumerable<object[]> ProvideTokenPairs() => from left in GetSingleTokens()
                                                               from right in GetSingleTokens()
                                                               where (left.kind, right.kind) switch
                                                               {
                                                                   (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => false,
                                                                   (_, SyntaxKind.NumberToken) => left.kind != SyntaxKind.IdentifierToken && !left.kind.IsKeyword(),
                                                                   (SyntaxKind.IdentifierToken, _) => right.kind != SyntaxKind.IdentifierToken && !right.kind.IsKeyword(),
                                                                   (_, SyntaxKind.IdentifierToken) => left.kind != SyntaxKind.IdentifierToken && !left.kind.IsKeyword(),
                                                                   (SyntaxKind.BangToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.BangToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.GreaterToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.GreaterToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.LessToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.LessToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.PipeToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.SlashToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.StarToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.MultiLineCommentTrivia) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.SingleLineCommentTrivia) => false,
                                                                   _ => !(left.kind.ToString().EndsWith("Keyword") && right.kind.ToString().EndsWith("Keyword"))
                                                               }
                                                               select new object[] { left.text, left.kind, right.text, right.kind };

    public static IEnumerable<object[]> ProvideSeparatedTokenPairs() => from left in GetSingleTokens()
                                                                        from right in GetSingleTokens()
                                                                        from separator in GetSeparators()
                                                                        select new object[]
                                                                        {
                                                                            left.text, left.kind, separator.text, separator.kind, right.text,
                                                                            right.kind
                                                                        };
}
