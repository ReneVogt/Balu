using Balu.Syntax;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{
    [Fact]
    public void Parser_PrefixIncrementExpression_CorrectExpressionSyntax()
    {
        var tree = SyntaxTree.Parse("a = ++b");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.AssignmentExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.EqualsToken);
        e.AssertNode(SyntaxKind.PrefixExpression);
        e.AssertToken(SyntaxKind.PlusPlusToken);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PrefixIncrementExpression_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("++b");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.PrefixExpression);
        e.AssertToken(SyntaxKind.PlusPlusToken);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PrefixDecrementExpression_CorrectExpressionSyntax()
    {
        var tree = SyntaxTree.Parse("a = --b");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.AssignmentExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.EqualsToken);
        e.AssertNode(SyntaxKind.PrefixExpression);
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PrefixDecrementExpression_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("--b");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.PrefixExpression);
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }
    [Fact]
    public void Parser_PrefixDecrementExpressionInLogicalBinaryExpression_CorrectExpressionStatementSyntax()
    {
        var tree = SyntaxTree.Parse("--a > b");
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.CompilationUnit);
        e.AssertNode(SyntaxKind.GlobalStatement);
        e.AssertNode(SyntaxKind.ExpressionStatement);
        e.AssertNode(SyntaxKind.BinaryExpression);
        e.AssertNode(SyntaxKind.PrefixExpression);
        e.AssertToken(SyntaxKind.MinusMinusToken);
        e.AssertToken(SyntaxKind.IdentifierToken, "a");
        e.AssertToken(SyntaxKind.GreaterToken);
        e.AssertNode(SyntaxKind.NameExpression);
        e.AssertToken(SyntaxKind.IdentifierToken, "b");
        e.AssertToken(SyntaxKind.EndOfFileToken);
    }

}
