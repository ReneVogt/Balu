using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.ParserTests;

public sealed class MissingTokenTests
{
    [Fact]
    public void Parser_MissingTypeIdentifier_HasZeroWidthSpan()
    {
        var tree = SyntaxTree.Parse("var x: = 1");
        var globalStatement = Assert.IsType<GlobalStatementSyntax>(Assert.Single(tree.Root.Members));
        var declaration = Assert.IsType<VariableDeclarationStatementSyntax>(globalStatement.Statement);
        var typeClause = Assert.IsType<TypeClauseSyntax>(declaration.TypeClause);

        Assert.True(typeClause.Identifier.IsMissing);
        Assert.Equal(new TextSpan(7, 0), typeClause.Identifier.Span);
        Assert.Equal(new TextSpan(7, 0), typeClause.Identifier.FullSpan);
        Assert.Equal(new TextSpan(7, 1), declaration.EqualsToken.Span);

        var diagnostic = Assert.Single(tree.Diagnostics);
        Assert.Equal(DiagnosticId.UnexpectedToken, diagnostic.Id);
        Assert.Equal(new TextSpan(7, 1), diagnostic.Location.Span);
    }

    [Fact]
    public void Parser_MissingDelimiter_HasZeroWidthSpan()
    {
        var tree = SyntaxTree.Parse("{ var x = (1 }");
        var globalStatement = Assert.IsType<GlobalStatementSyntax>(Assert.Single(tree.Root.Members));
        var block = Assert.IsType<BlockStatementSyntax>(globalStatement.Statement);
        var declaration = Assert.IsType<VariableDeclarationStatementSyntax>(Assert.Single(block.Statements));
        var expression = Assert.IsType<ParenthesizedExpressionSyntax>(declaration.Expression);

        Assert.True(expression.ClosedParenthesisToken.IsMissing);
        Assert.Equal(new TextSpan(13, 0), expression.ClosedParenthesisToken.Span);
        Assert.Equal(new TextSpan(13, 0), expression.ClosedParenthesisToken.FullSpan);
        Assert.Equal(new TextSpan(13, 1), block.ClosedBraceToken.Span);

        var diagnostic = Assert.Single(tree.Diagnostics);
        Assert.Equal(DiagnosticId.UnexpectedToken, diagnostic.Id);
        Assert.Equal(new TextSpan(13, 1), diagnostic.Location.Span);
    }
}
