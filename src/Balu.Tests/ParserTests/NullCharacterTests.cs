using System.Linq;
using Balu.Diagnostics;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{
    [Fact]
    public void Parser_PreservesEmbeddedNullCharacterAndFollowingCode()
    {
        const string text = "println()\0println()";

        var tree = SyntaxTree.Parse(text);

        Assert.Equal(2, tree.Root.Members.Length);
        var diagnostic = Assert.Single(tree.Diagnostics);
        Assert.Equal(DiagnosticId.UnexpectedToken, diagnostic.Id);
        var secondMember = tree.Root.Members[1];
        var skipped = Assert.Single(secondMember.FirstToken.LeadingTrivia.Where(trivia => trivia.Kind == SyntaxKind.SkippedTextTrivia));
        Assert.Equal("\0", skipped.Text);
        Assert.Equal(text.Length, tree.Root.EndOfFileToken.Span.Start);
    }
}
