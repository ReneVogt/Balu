using Balu.Syntax;
using System.Collections.Generic;
using System.Linq;
using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ParserTests;

public partial class ParserTests
{

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
            e.AssertNode(SyntaxKind.CompilationUnit);
            e.AssertNode(SyntaxKind.GlobalStatement);
            e.AssertNode(SyntaxKind.ExpressionStatement);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(SyntaxKind.EndOfFileToken);
        }
        else
        {
            using var e = new SyntaxTreeAsserter(tree.Root);
            e.AssertNode(SyntaxKind.CompilationUnit);
            e.AssertNode(SyntaxKind.GlobalStatement);
            e.AssertNode(SyntaxKind.ExpressionStatement);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(SyntaxKind.EndOfFileToken);
        }
    }

    public static IEnumerable<object[]> ProvideUnaryBinaryOperatorPairs() => from unary in SyntaxFacts.GetUnaryOperators()
                                                                             from binary in SyntaxFacts.GetBinaryOperators()
                                                                             select new object[] { unary, binary };
}
