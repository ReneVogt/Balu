using Balu.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
    [Fact]
    public void Lexer_Tests_CoveringAllTokens()
    {
        var nonTestingKinds = new[] { SyntaxKind.EndOfFileToken, SyntaxKind.BadToken };
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
}
