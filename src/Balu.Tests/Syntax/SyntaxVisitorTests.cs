using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax;

public class SyntaxVisitorTests
{
    [Fact]
    public void SyntaxVisitor_ImplementsAllVirtualVisits()
    {
        var visitMethods = from method in typeof(SyntaxVisitor).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                           let match = visitMethodRegex.Match(method.Name)
                           where match.Success && match.Groups.Count == 2 && method.GetParameters().Length == 1 && method.ReturnType == typeof(SyntaxNode) && method.IsVirtual
                           select method;
        var expectedTypes = GetAllSyntaxNodeTypes().ToList();
        var missingTypes = expectedTypes.Where(type => !visitMethods.Any(method =>
                                            {
                                                if (type == typeof(SyntaxToken))
                                                    return method.Name == "VisitToken" &&
                                                           method.GetParameters()[0].ParameterType == typeof(SyntaxToken);
                                                var expectedMethodName = $"Visit{type.Name[..^6]}"; // remove "Syntax"
                                                return method.Name == expectedMethodName && method.GetParameters()[0].ParameterType == type;

                                            })
                                        )
                                        .ToList();
        Assert.Empty(missingTypes);

    }
    [Fact]
    public void SyntaxVisitor_AcceptAndGetChildrenTestedForAllSyntaxNodes()
    {
        var testedNames = from method in typeof(SyntaxVisitorTests).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          let match = testingMethodRegex.Match(method.Name)
                          where match.Success && match.Groups.Count == 2 && method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.GetCustomAttribute<FactAttribute>() is not null
                          select match.Groups[1].Value;

        var expectedNames = GetAllSyntaxNodeTypes().Select(type => type.Name);
        var missingNames = expectedNames.Except(testedNames).ToList();
        Assert.Empty(missingNames);
    }

    [Fact]
    public void SyntaxVisitor_AssignmentExpressionSyntax_AcceptVisitsChildren()
    {
        var literal = ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty));
        var equalsToken = SyntaxToken.Equals(default);
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var assignment = ExpressionSyntax.Assignment(identifier, equalsToken, literal);
        AssertVisits(assignment);
    }
    [Fact]
    public void SyntaxVisitor_CallExpressionSyntax_AcceptVisitsChildren()
    {
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var open = SyntaxToken.OpenParenthesis(default);
        var parameter = ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(default);
        var call = ExpressionSyntax.Call(identifier, open, new (new SyntaxNode[] { parameter }), close);
        AssertVisits(call);
    }
    [Fact]
    public void SyntaxVisitor_BinaryExpressionSyntax_AcceptVisitsChildren()
    {
        var left = ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty));
        var operatorToken = SyntaxToken.Plus(default);
        var right = ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty));
        var binary = ExpressionSyntax.Binary(left, operatorToken, right);
        AssertVisits(binary);
    }
    [Fact]
    public void SyntaxVisitor_BlockStatementSyntax_AcceptVisitsChildren()
    {
        var openBraceToken = SyntaxToken.OpenBrace(default);
        var statements = Enumerable.Range(0, 5)
                                   .Select(_ => StatementSyntax.ExpressionStatement(
                                               ExpressionSyntax.Name(SyntaxToken.Identifier(default, string.Empty))));
        var closedBraceToken = SyntaxToken.ClosedBrace(default);
        var statement = StatementSyntax.BlockStatement(openBraceToken, statements, closedBraceToken);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_CompilationUnitSyntax_AcceptVisitsChildren()
    {
        var statement = StatementSyntax.ExpressionStatement(ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty)));
        var eof = SyntaxToken.EndOfFile(default);
        var compilationUnit = SyntaxNode.CompilationUnit(statement, eof);
        AssertVisits(compilationUnit);
    }
    [Fact]
    public void SyntaxVisitor_ElseClauseSyntax_AcceptVisitsChildren()
    {
        var elseToken = SyntaxToken.ElseKeyword(default);
        var statement = StatementSyntax.ExpressionStatement(ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty)));
        var elseClause = SyntaxNode.Else(elseToken, statement);
        AssertVisits(elseClause);
    }
    [Fact]
    public void SyntaxVisitor_ExpressionStatementSyntax_AcceptVisitsChildren()
    {
        var expression = ExpressionSyntax.Literal(SyntaxToken.Number(default, 0, string.Empty));
        var statement = StatementSyntax.ExpressionStatement(expression);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_IfStatementSyntax_AcceptVisitsChildren()
    {
        var ifToken = SyntaxToken.IfKeyword(default);
        var condition = ExpressionSyntax.Literal(SyntaxToken.TrueKeyword(default));
        var thenStatement = StatementSyntax.ExpressionStatement(condition);
        var elseToken = SyntaxToken.ElseKeyword(default);
        var elseStatement = StatementSyntax.ExpressionStatement(condition);
        var elseClause = SyntaxNode.Else(elseToken, elseStatement);
        var statement = StatementSyntax.IfStatement(ifToken, condition, thenStatement, elseClause);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_LiteralExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(default, string.Empty);
        var expression = ExpressionSyntax.Literal(token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_NameExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(default, string.Empty);
        var expression = ExpressionSyntax.Name(token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_ParenthesizedExpressionSyntax_AcceptVisitsChildren()
    {
        var open = SyntaxToken.OpenParenthesis(default);
        var inner = ExpressionSyntax.Name(SyntaxToken.Identifier(default, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(default);
        var expression = ExpressionSyntax.Parenthesized(open, inner, close);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_UnaryExpressionSyntax_AcceptVisitsChildren()
    {
        var operatorToken = SyntaxToken.Bang(default);
        var operand = ExpressionSyntax.Literal(SyntaxToken.TrueKeyword(default));
        var expression = ExpressionSyntax.Unary(operatorToken, operand);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_VariableDeclarationStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.LetKeyword(default);
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var equals = SyntaxToken.Equals(default);
        var expression = ExpressionSyntax.Literal(SyntaxToken.TrueKeyword(default));
        var typeClause = SyntaxNode.Type(SyntaxToken.Colon(default), SyntaxToken.Identifier(default, string.Empty));
        var statement = StatementSyntax.VariableDeclarationStatement(keyword, identifier, equals, expression, typeClause);
        AssertVisits(statement);
        statement = StatementSyntax.VariableDeclarationStatement(keyword, identifier, equals, expression, null);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_WhileStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.WhileKeyword(default);
        var condition = ExpressionSyntax.Literal(SyntaxToken.TrueKeyword(default));
        var body = StatementSyntax.ExpressionStatement(condition);
        var statement = StatementSyntax.WhileStatement(keyword, condition, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_DoWhileStatementSyntax_AcceptVisitsChildren()
    {
        var doKeyword = SyntaxToken.DoKeyword(default);
        var condition = ExpressionSyntax.Literal(SyntaxToken.TrueKeyword(default));
        var body = StatementSyntax.ExpressionStatement(condition);
        var whileKeyword = SyntaxToken.WhileKeyword(default);
        var statement = StatementSyntax.DoWhileStatement(doKeyword, body, whileKeyword, condition);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_ForStatementSyntax_AcceptVisitsChildren()
    {
        var forKeyword = SyntaxToken.ForKeyword(default);
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var equals = SyntaxToken.Equals(default);
        var dummy = SyntaxToken.TrueKeyword(default);
        var lowerBound = ExpressionSyntax.Literal(dummy);
        var toKeyWord = SyntaxToken.ToKeyword(default);
        var upperBound = ExpressionSyntax.Literal(dummy);
        var body = StatementSyntax.ExpressionStatement(upperBound);
        var statement = StatementSyntax.ForStatement(forKeyword, identifier, equals, lowerBound, toKeyWord, upperBound, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_TypeClauseSyntax_AcceptVisitsChildren()
    {
        var colon = SyntaxToken.Colon(default);
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var typeClause = SyntaxNode.Type(colon, identifier);
        AssertVisits(typeClause);
    }

    static readonly Regex visitMethodRegex = new("Visit(.*?)", RegexOptions.Compiled);
    static readonly Regex testingMethodRegex = new("SyntaxVisitor_(.*?)_AcceptVisitsChildren", RegexOptions.Compiled);
    static IEnumerable<Type> GetAllSyntaxNodeTypes() => from type in typeof(SyntaxNode).Assembly.GetExportedTypes()
                                                        where type != typeof(SyntaxToken) && typeof(SyntaxNode).IsAssignableFrom(type) && type.IsPublic && !type.IsAbstract
                                                        select type;

    static void AssertVisits(SyntaxNode node)
    {
        var visitor = new TestVisitor();
        visitor.Visit(node);
        var expected = node.Children.ToList();
        expected.Insert(0, node);
        Assert.Equal(expected, visitor.Visited);
    }
    sealed class TestVisitor : SyntaxVisitor
    {
        bool parented;
        public List<SyntaxNode> Visited { get; } = new();
        public override SyntaxNode Visit(SyntaxNode node)
        {
            Visited.Add(node);
            if (parented) return node;
            parented = true;
            return base.Visit(node);
        }
    }
}
