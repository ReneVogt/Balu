using Balu.Syntax;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
    [Fact]
    public void Lexer_Trivia_AsssignedCorrectly()
    {
        const string code = @"
        /*Multiline comment
          ends here*/
        a // single line comment
        // next single line comment
        /* multi */ b // single
        c /* inline */ d /* next */ e
        /* inline */ f /* next */ g /* end*/
        // end of file must be tested in parser tests, parsetokens does not return the eof token";

        var tokens = SyntaxTree.ParseTokens(code);
        Assert.Equal(7, tokens.Length);

        // a
        Assert.Equal(5, tokens[0].LeadingTrivia.Length);
        Assert.Equal(SyntaxKind.LineBreakTrivia, tokens[0].LeadingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[0].LeadingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[0].LeadingTrivia[2].Kind);
        Assert.Equal(SyntaxKind.LineBreakTrivia, tokens[0].LeadingTrivia[3].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[0].LeadingTrivia[4].Kind);
        Assert.Equal(2, tokens[0].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[0].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, tokens[0].TrailingTrivia[1].Kind);

        // b
        Assert.Equal(5, tokens[1].LeadingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[1].LeadingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, tokens[1].LeadingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[1].LeadingTrivia[2].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[1].LeadingTrivia[3].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[1].LeadingTrivia[4].Kind);
        Assert.Equal(2, tokens[1].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[1].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, tokens[1].TrailingTrivia[1].Kind);

        // c
        Assert.Single(tokens[2].LeadingTrivia);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[2].LeadingTrivia[0].Kind);
        Assert.Equal(3, tokens[2].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[2].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[2].TrailingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[2].TrailingTrivia[2].Kind);

        // d
        Assert.Empty(tokens[3].LeadingTrivia);
        Assert.Equal(3, tokens[3].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[3].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[3].TrailingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[3].TrailingTrivia[2].Kind);

        // e
        Assert.Empty(tokens[4].LeadingTrivia);
        Assert.Single(tokens[4].TrailingTrivia);
        Assert.Equal(SyntaxKind.LineBreakTrivia, tokens[4].TrailingTrivia[0].Kind);

        // f
        Assert.Equal(3, tokens[5].LeadingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[5].LeadingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[5].LeadingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[5].LeadingTrivia[2].Kind);
        Assert.Equal(3, tokens[5].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[5].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[5].TrailingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[5].TrailingTrivia[2].Kind);

        // g
        Assert.Empty(tokens[6].LeadingTrivia);
        Assert.Equal(3, tokens[5].TrailingTrivia.Length);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, tokens[6].TrailingTrivia[0].Kind);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, tokens[6].TrailingTrivia[1].Kind);
        Assert.Equal(SyntaxKind.LineBreakTrivia, tokens[6].TrailingTrivia[2].Kind);
    }
    [Fact]
    public void Lexer_Trivia_ReportsUnterminatedMultilineComment()
    {
        const string code = @"[/*] hera starts some comment
but it's not terminated";
        code.AssertLexerDiagnostics("Unterminated multiline comment.");
    }
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void Lexer_Trivia_SingleLineCommentRecognizesAllLineEndings(string lineEnding)
    {
        var tokens = SyntaxTree.ParseTokens("a// comment" + lineEnding + "  b");

        Assert.Equal(2, tokens.Length);
        var comment = Assert.Single(tokens[0].TrailingTrivia);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, comment.Kind);
        Assert.Equal("// comment" + lineEnding, comment.Text);
        var whitespace = Assert.Single(tokens[1].LeadingTrivia);
        Assert.Equal(SyntaxKind.WhiteSpaceTrivia, whitespace.Kind);
        Assert.Equal("  ", whitespace.Text);
    }
    [Fact]
    public void Lexer_Trivia_SingleLineCommentAllowsEmbeddedNullCharacter()
    {
        var tokens = SyntaxTree.ParseTokens("a// x\0y\rb");

        Assert.Equal(2, tokens.Length);
        var comment = Assert.Single(tokens[0].TrailingTrivia);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, comment.Kind);
        Assert.Equal("// x\0y\r", comment.Text);
        Assert.Equal("b", tokens[1].Text);
    }
    [Fact]
    public void Lexer_Trivia_MultiLineCommentAllowsEmbeddedNullCharacter()
    {
        var tokens = SyntaxTree.ParseTokens("a/* x\0y */b");

        Assert.Equal(2, tokens.Length);
        var comment = Assert.Single(tokens[0].TrailingTrivia);
        Assert.Equal(SyntaxKind.MultiLineCommentTrivia, comment.Kind);
        Assert.Equal("/* x\0y */", comment.Text);
        Assert.Equal("b", tokens[1].Text);
    }
}
