using Balu.Syntax;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{
    [Fact]
    public void Parser_PostfixIncrementExpression_CorrectExpressionSyntax()
    {
        var tree = SyntaxTree.Parse("a = b++");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.AssignmentExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.EqualsToken);
        e.AssertNode(SyntaxKind.PostfixExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.PlusPlusToken);
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PostfixIncrementExpression_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("b++");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.PostfixExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.PlusPlusToken);
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PostfixDecrementExpression_CorrectExpressionSyntax()
    {
        var tree = SyntaxTree.Parse("a = b--");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.AssignmentExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.EqualsToken);
        e.AssertNode(SyntaxKind.PostfixExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PostfixDecrementExpression_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("b--");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.PostfixExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PostfixDecrementExpressionInBinary_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("a = b-- c");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.AssignmentExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.EqualsToken);
        e.AssertNode(SyntaxKind.PostfixExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.NameExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "c");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
}
