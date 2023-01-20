using Balu.Syntax;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{
    [Fact]
    public void Parser_Trivia_ParsedCorrectly()
    {
        const string text = @"
            /* here this is a 
               multiline comment */
            println( /*inside*/ ) // single line after println
            // single line before input
              input() /* after*/ print() /* after print */
            // and here a bad token
            [?]
            call() 
            /* and a multiline before eof */
";
        using var e = new SyntaxTreeAsserter(text, expectedDiagnostics: "Unexpected token '?'.", includeTrivia: true);
        e.AssertNode(SyntaxKind.CompilationUnit);

        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.CallExpression);
        e.AssertLeadingTrivia(SyntaxKind.MultiLineCommentTrivia);
        e.AssertLeadingTrivia(SyntaxKind.LineBreakTrivia);
        e.AssertToken(SyntaxKind.IdentifierToken, "println");
        e.AssertToken(SyntaxKind.OpenParenthesisToken);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertTrailingTrivia(SyntaxKind.MultiLineCommentTrivia, "/*inside*/");
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertToken(SyntaxKind.ClosedParenthesisToken);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertTrailingTrivia(SyntaxKind.SingleLineCommentTrivia);

        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.CallExpression);
        e.AssertLeadingTrivia(SyntaxKind.SingleLineCommentTrivia);
        e.AssertLeadingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertToken(SyntaxKind.IdentifierToken, "input");
        e.AssertToken(SyntaxKind.OpenParenthesisToken);
        e.AssertToken(SyntaxKind.ClosedParenthesisToken);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertTrailingTrivia(SyntaxKind.MultiLineCommentTrivia);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);

        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.CallExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "print");
        e.AssertToken(SyntaxKind.OpenParenthesisToken);
        e.AssertToken(SyntaxKind.ClosedParenthesisToken);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertTrailingTrivia(SyntaxKind.MultiLineCommentTrivia);
        e.AssertTrailingTrivia(SyntaxKind.LineBreakTrivia);

        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.CallExpression);
        e.AssertLeadingTrivia(SyntaxKind.SingleLineCommentTrivia);
        e.AssertLeadingTrivia(SyntaxKind.SkippedTextTrivia, "?");
        e.AssertLeadingTrivia(SyntaxKind.LineBreakTrivia);
        e.AssertToken(SyntaxKind.IdentifierToken, "call");
        e.AssertToken(SyntaxKind.OpenParenthesisToken);
        e.AssertToken(SyntaxKind.ClosedParenthesisToken);
        e.AssertTrailingTrivia(SyntaxKind.WhiteSpaceTrivia);
        e.AssertTrailingTrivia(SyntaxKind.LineBreakTrivia);

        e.AssertLeadingTrivia(SyntaxKind.MultiLineCommentTrivia, "/* and a multiline before eof */");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
}
