using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
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
}
