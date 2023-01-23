using Balu.Syntax;
using System.Collections.Generic;
using System.Linq;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
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
            e.AssertNode(SyntaxKind.CompilationUnit);
            e.AssertNode(SyntaxKind.GlobalStatement);
            e.AssertNode(SyntaxKind.ExpressionStatement);
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
            e.AssertToken(SyntaxKind.EndOfFileToken);
        }
        else
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.CompilationUnit);
            e.AssertNode(SyntaxKind.GlobalStatement);
            e.AssertNode(SyntaxKind.ExpressionStatement);
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
            e.AssertToken(SyntaxKind.EndOfFileToken);
        }
    }
    public static IEnumerable<object[]> ProvideBinaryOperatorPairs() => from left in SyntaxFacts.GetBinaryOperators()
                                                                        from right in SyntaxFacts.GetBinaryOperators()
                                                                        select new object[] { left, right };
}
