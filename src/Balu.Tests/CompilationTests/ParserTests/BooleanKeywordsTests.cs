using Balu.Syntax;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{

    [Theory]
    [InlineData(SyntaxKind.TrueKeyword, true)]
    [InlineData(SyntaxKind.FalseKeyword, false)]
    public void Parser_BooleanKeywords_CorrectValues(SyntaxKind kind, object? value)
    {
        var text = kind.GetText()!;
        var tree = SyntaxTree.Parse(text);
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.LiteralExpression);
        e.AssertToken(kind, text, value);
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
}
