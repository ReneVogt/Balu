using Balu.Syntax;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void Parser_ReturnStatement_RecognizesAllLineEndings(string lineEnding)
    {
        var tree = SyntaxTree.Parse("function test() { return" + lineEnding + "1 }");

        Assert.Empty(tree.Diagnostics);
        var function = Assert.IsType<FunctionDeclarationSyntax>(Assert.Single(tree.Root.Members));
        Assert.Equal(2, function.Body.Statements.Length);
        var returnStatement = Assert.IsType<ReturnStatementSyntax>(function.Body.Statements[0]);
        Assert.Null(returnStatement.Expression);
        Assert.IsType<ExpressionStatementSyntax>(function.Body.Statements[1]);
    }
}
