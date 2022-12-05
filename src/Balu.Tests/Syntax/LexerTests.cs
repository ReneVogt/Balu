using Balu.Syntax;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Balu.Tests.Syntax;

public class LexerTests
{
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
    public static IEnumerable<(string text, SyntaxKind kind)> GetSingleTokens() => new (string text, SyntaxKind kind)[]
    {
        ("01", kind: SyntaxKind.NumberToken),
        ("123", kind: SyntaxKind.NumberToken),
        ("987654321", kind: SyntaxKind.NumberToken),

        ("+", kind: SyntaxKind.PlusToken),
        ("-", kind: SyntaxKind.MinusToken),
        ("*", kind: SyntaxKind.StarToken),
        ("/", kind: SyntaxKind.SlashToken),
        ("(", kind: SyntaxKind.OpenParenthesisToken),
        (")", kind: SyntaxKind.ClosedParenthesisToken),
        ("=", kind: SyntaxKind.EqualsToken),
        ("!", kind: SyntaxKind.BangToken),
        ("&&", kind: SyntaxKind.AmpersandAmpersandToken),
        ("||", kind: SyntaxKind.PipePipeToken),
        ("==", kind: SyntaxKind.EqualsEqualsToken),
        ("!=", kind: SyntaxKind.BangEqualsToken),

        ("myNameIs", kind: SyntaxKind.IdentifierToken),
        ("x", kind: SyntaxKind.IdentifierToken),

        ("true", kind: SyntaxKind.TrueKeyword),
        ("false", kind: SyntaxKind.FalseKeyword)
    };
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
                                                               where !(Equals(SyntaxKind.NumberToken, left.kind) &&
                                                                       Equals(SyntaxKind.NumberToken, right.kind)) &&
                                                                     !((Equals(SyntaxKind.IdentifierToken, left.kind) ||
                                                                        Equals(SyntaxKind.TrueKeyword, left.kind) ||
                                                                        Equals(SyntaxKind.FalseKeyword, left.kind)) &&
                                                                       (Equals(SyntaxKind.IdentifierToken, right.kind) ||
                                                                        Equals(SyntaxKind.TrueKeyword, right.kind) ||
                                                                        Equals(SyntaxKind.FalseKeyword, right.kind))) &&
                                                                     !(Equals(SyntaxKind.BangToken, left.kind) &&
                                                                       (Equals(SyntaxKind.EqualsEqualsToken, right.kind) ||
                                                                        Equals(SyntaxKind.EqualsToken, right.kind))) &&
                                                                     !(Equals(SyntaxKind.EqualsToken, left.kind) &&
                                                                       (Equals(SyntaxKind.EqualsEqualsToken, right.kind) ||
                                                                        Equals(SyntaxKind.EqualsToken, right.kind)))
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
