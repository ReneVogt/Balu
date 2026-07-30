using System.Linq;
using Balu.Authoring;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.ParserTests;

public sealed class VariableDeclarationStatementTests
{
    [Fact]
    public void Parser_TypedVariableDeclaration_PreservesSourceOrder()
    {
        const string text = "var i : any = 12";
        var tree = SyntaxTree.Parse(text);
        var globalStatement = Assert.IsType<GlobalStatementSyntax>(Assert.Single(tree.Root.Members));
        var declaration = Assert.IsType<VariableDeclarationStatementSyntax>(globalStatement.Statement);

        Assert.Equal(5, declaration.ChildrenCount);
        Assert.Same(declaration.KeywordToken, declaration.GetChild(0));
        Assert.Same(declaration.IdentifierToken, declaration.GetChild(1));
        Assert.Same(declaration.TypeClause, declaration.GetChild(2));
        Assert.Same(declaration.EqualsToken, declaration.GetChild(3));
        Assert.Same(declaration.Expression, declaration.GetChild(4));
        Assert.Equal(new TextSpan(0, text.Length), declaration.Span);
        Assert.Equal(new TextSpan(0, text.Length), declaration.FullSpan);
        Assert.Same(declaration.Expression.LastToken, declaration.LastToken);

        var classifiedSpans = Classifier.Classify(tree, new TextSpan(0, text.Length));
        Assert.Equal(classifiedSpans.Select(span => span.Span.Start).OrderBy(start => start),
                     classifiedSpans.Select(span => span.Span.Start));
    }
}
