using Balu.Syntax;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Balu.Tests.Syntax;
public class ParserTests
{

    [Theory]
    [MemberData(nameof(ProvideBinaryOperatorPairs))]
    public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind left, SyntaxKind right)
    {
        var leftPrecedence = left.BinaryOperatorPrecedence();
        var leftText = left.GetText()!;
        var rightPrecedence = right.BinaryOperatorPrecedence();
        var rightText = right.GetText()!;

        var text = $"a {leftText} b {rightText} c";
        var tree = SyntaxTree.Parse(text);

        if (leftPrecedence >= rightPrecedence)
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(left, leftText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(right, rightText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
        else
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(left, leftText);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(right, rightText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "c");

        }
    }

    [Theory]
    [MemberData(nameof(ProvideUnaryBinaryOperatorPairs))]
    public void Parser_UnaryExpression_HonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
    {
        var unaryPrecedence = unaryKind.UnaryOperatorPrecedence();
        var unaryText = unaryKind.GetText()!;
        var binaryPrecedence = binaryKind.BinaryOperatorPrecedence();
        var binaryText = binaryKind.GetText()!;

        var text = $"{unaryText} a {binaryText} b";
        var tree = SyntaxTree.Parse(text);

        if (unaryPrecedence >= binaryPrecedence)
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
        else
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
    }

    [Theory]
    [InlineData(SyntaxKind.TrueKeyword, true)]
    [InlineData(SyntaxKind.FalseKeyword, false)]
    public void Parser_BooleanKeywords_CorrectValues(SyntaxKind kind, object? value)
    {
        var text = kind.GetText()!;
        var tree = SyntaxTree.Parse(text);
        using var e = new SyntaxTreeAsserter(tree.Root);
        e.AssertNode(SyntaxKind.LiteralExpression);
        e.AssertToken(kind, text, value);
    }
    public static IEnumerable<object[]> ProvideBinaryOperatorPairs() => from left in SyntaxFacts.GetBinaryOperators()
                                                                        from right in SyntaxFacts.GetBinaryOperators()
                                                                        select new object[] { left, right };
    public static IEnumerable<object[]> ProvideUnaryBinaryOperatorPairs() => from unary in SyntaxFacts.GetUnaryOperators()
                                                                        from binary in SyntaxFacts.GetBinaryOperators()
                                                                        select new object[] { unary, binary };
}
