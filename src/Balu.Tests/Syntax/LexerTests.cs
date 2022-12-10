using Balu.Syntax;
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
        var nonTestingKinds = new[] { SyntaxKind.EndOfFileToken, SyntaxKind.BadToken };
        var allTokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                .Cast<SyntaxKind>()
                                .Where(kind => kind.ToString().EndsWith("Keyword") || kind.ToString().EndsWith("Token"))
                                .Except(nonTestingKinds);
        var testedTokens = ProvideSingleTokens().Select(x => (SyntaxKind)x[1]).Distinct();
        var untestedTokenKinds = allTokenKinds.Except(testedTokens).ToList();
        Assert.Empty(untestedTokenKinds);
    }
    [Fact]
    public void Lexer_Lexes_EmptyString()
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
        var tokens = SyntaxTree.ParseTokens(text1 + text2).ToArray();
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
        var tokens = SyntaxTree.ParseTokens(text1 + separatorText + text2).ToArray();
        Assert.Equal(3, tokens.Length);
        Assert.Equal(kind1, tokens[0].Kind);
        Assert.Equal(text1, tokens[0].Text);
        Assert.Equal(separatorKind, tokens[1].Kind);
        Assert.Equal(separatorText, tokens[1].Text);
        Assert.Equal(kind2, tokens[2].Kind);
        Assert.Equal(text2, tokens[2].Text);
    }

    public static IEnumerable<object[]> ProvideSingleTokens() => GetSingleTokens().Concat(GetSeparators()).Select(x => new object[] { x.text, x.kind });
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
                    ("x", kind: SyntaxKind.IdentifierToken),
                    ("true", kind: SyntaxKind.TrueKeyword),
                    ("false", kind: SyntaxKind.FalseKeyword)
                });
    static IEnumerable<(string text, SyntaxKind kind)> GetSeparators() => new (string text, SyntaxKind kind)[]
    {
        (" ", kind: SyntaxKind.WhiteSpaceToken),
        ("  ", kind: SyntaxKind.WhiteSpaceToken),
        ("\r", kind: SyntaxKind.WhiteSpaceToken),
        ("\n", kind: SyntaxKind.WhiteSpaceToken),
        ("\r\n ", kind: SyntaxKind.WhiteSpaceToken),
        ("\t\v", kind: SyntaxKind.WhiteSpaceToken)
    };

    public static IEnumerable<object[]> ProvideTokenPairs() => from left in GetSingleTokens()
                                                               from right in GetSingleTokens()
                                                               where (left.kind, right.kind) switch
                                                               {
                                                                   (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => false,
                                                                   (SyntaxKind.IdentifierToken, _) => right.kind != SyntaxKind.IdentifierToken && !right.kind.ToString().EndsWith("Keyword"),
                                                                   (_, SyntaxKind.IdentifierToken) => left.kind != SyntaxKind.IdentifierToken && !left.kind.ToString().EndsWith("Keyword"),
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
