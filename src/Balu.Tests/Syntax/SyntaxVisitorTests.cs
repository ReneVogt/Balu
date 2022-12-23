using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax;

public class SyntaxVisitorTests
{
    static readonly SyntaxTree dummyTree = SyntaxTree.Parse(string.Empty);

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
        var literal = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty));
        var equalsToken = SyntaxToken.Equals(dummyTree, default);
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var assignment = new AssignmentExpressionSyntax(dummyTree, identifier, equalsToken, literal);
        AssertVisits(assignment);
    }
    [Fact]
    public void SyntaxVisitor_CallExpressionSyntax_AcceptVisitsChildren()
    {
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var open = SyntaxToken.OpenParenthesis(dummyTree, default);
        var parameter = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(dummyTree, default);
        var call = new CallExpressionSyntax(dummyTree, identifier, open, new (new SyntaxNode[] { parameter }.ToImmutableArray()), close);
        AssertVisits(call);
    }
    [Fact]
    public void SyntaxVisitor_BinaryExpressionSyntax_AcceptVisitsChildren()
    {
        var left = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty));
        var operatorToken = SyntaxToken.Plus(dummyTree, default);
        var right = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty));
        var binary = new BinaryExpressionSyntax(dummyTree, left, operatorToken, right);
        AssertVisits(binary);
    }
    [Fact]
    public void SyntaxVisitor_BlockStatementSyntax_AcceptVisitsChildren()
    {
        var openBraceToken = SyntaxToken.OpenBrace(dummyTree, default);
        var statements = Enumerable.Range(0, 5)
                                   .Select(_ => new ExpressionStatementSyntax(dummyTree,
                                                                              new NameExpressionSyntax(
                                                                                  dummyTree, SyntaxToken.Identifier(dummyTree, default, string.Empty))));
        var closedBraceToken = SyntaxToken.ClosedBrace(dummyTree, default);
        var statement = new BlockStatementSyntax(dummyTree, openBraceToken, statements.ToImmutableArray<StatementSyntax>(), closedBraceToken);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_CompilationUnitSyntax_AcceptVisitsChildren()
    {
        var statement = new ExpressionStatementSyntax(dummyTree, new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty)));
        var eof = SyntaxToken.EndOfFile(dummyTree, default);
        var compilationUnit = new CompilationUnitSyntax(dummyTree, new MemberSyntax[]{new GlobalStatementSyntax(dummyTree, statement) }.ToImmutableArray(), eof);
        AssertVisits(compilationUnit);
    }
    [Fact]
    public void SyntaxVisitor_GlobalStatementSyntax_AcceptVisitsChildren()
    {
        var statement = new ExpressionStatementSyntax(dummyTree, new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty)));
        var globalStatement = new GlobalStatementSyntax(dummyTree, statement);
        AssertVisits(globalStatement);
    }
    [Fact]
    public void SyntaxVisitor_FunctionDeclarationSyntax_AcceptVisitsChildren()
    {
        var functionKeyword = SyntaxToken.FunctionKeyword(dummyTree, default);
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var openParenthesis = SyntaxToken.OpenParenthesis(dummyTree, default);
        var type = new TypeClauseSyntax(dummyTree, identifier, identifier);
        var parameters =
            new SeparatedSyntaxList<ParameterSyntax>(new SyntaxNode[] { new ParameterSyntax(dummyTree, identifier, type) }
                                                         .ToImmutableArray());
        var closeParenthesis = SyntaxToken.OpenParenthesis(dummyTree, default);
        var body = new BlockStatementSyntax(dummyTree, SyntaxToken.OpenBrace(dummyTree, default), ImmutableArray<StatementSyntax>.Empty, SyntaxToken.ClosedBrace(dummyTree, default));
        var functionDelcaration = new FunctionDeclarationSyntax(dummyTree, functionKeyword, identifier, openParenthesis, parameters, closeParenthesis, type, body);
        AssertVisits(functionDelcaration);
    }
    [Fact]
    public void SyntaxVisitor_ReturnStatementSyntax_AcceptVisitsChildren()
    {
        var returnKeyword = SyntaxToken.ReturnKeyword(dummyTree, default);
        var expression = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, "0"));
        var returnStatement = new ReturnStatementSyntax(dummyTree, returnKeyword, expression);
        AssertVisits(returnStatement);
    }
    [Fact]
    public void SyntaxVisitor_ParameterSyntax_AcceptVisitsChildren()
    {
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var type = new TypeClauseSyntax(dummyTree, SyntaxToken.Colon(dummyTree, default), SyntaxToken.Identifier(dummyTree, default, string.Empty));
        var parameter = new ParameterSyntax(dummyTree, identifier, type);
        AssertVisits(parameter);
    }
    [Fact]
    public void SyntaxVisitor_ElseClauseSyntax_AcceptVisitsChildren()
    {
        var elseToken = SyntaxToken.ElseKeyword(dummyTree, default);
        var statement = new ExpressionStatementSyntax(dummyTree, new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty)));
        var elseClause = new ElseClauseSyntax(dummyTree, elseToken, statement);
        AssertVisits(elseClause);
    }
    [Fact]
    public void SyntaxVisitor_ExpressionStatementSyntax_AcceptVisitsChildren()
    {
        var expression = new LiteralExpressionSyntax(dummyTree, SyntaxToken.Number(dummyTree, default, 0, string.Empty));
        var statement = new ExpressionStatementSyntax(dummyTree, expression);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_IfStatementSyntax_AcceptVisitsChildren()
    {
        var ifToken = SyntaxToken.IfKeyword(dummyTree, default);
        var condition = new LiteralExpressionSyntax(dummyTree, SyntaxToken.TrueKeyword(dummyTree, default));
        var thenStatement = new ExpressionStatementSyntax(dummyTree, condition);
        var elseToken = SyntaxToken.ElseKeyword(dummyTree, default);
        var elseStatement = new ExpressionStatementSyntax(dummyTree, condition);
        var elseClause = new ElseClauseSyntax(dummyTree, elseToken, elseStatement);
        var statement = new IfStatementSyntax(dummyTree, ifToken, condition, thenStatement, elseClause);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_LiteralExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var expression = new LiteralExpressionSyntax(dummyTree, token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_NameExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var expression = new NameExpressionSyntax(dummyTree, token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_ParenthesizedExpressionSyntax_AcceptVisitsChildren()
    {
        var open = SyntaxToken.OpenParenthesis(dummyTree, default);
        var inner = new NameExpressionSyntax(dummyTree, SyntaxToken.Identifier(dummyTree, default, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(dummyTree, default);
        var expression = new ParenthesizedExpressionSyntax(dummyTree, open, inner, close);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_UnaryExpressionSyntax_AcceptVisitsChildren()
    {
        var operatorToken = SyntaxToken.Bang(dummyTree, default);
        var operand = new LiteralExpressionSyntax(dummyTree, SyntaxToken.TrueKeyword(dummyTree, default));
        var expression = new UnaryExpressionSyntax(dummyTree, operatorToken, operand);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_VariableDeclarationStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.LetKeyword(dummyTree, default);
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var equals = SyntaxToken.Equals(dummyTree, default);
        var expression = new LiteralExpressionSyntax(dummyTree, SyntaxToken.TrueKeyword(dummyTree, default));
        var typeClause = new TypeClauseSyntax(dummyTree, SyntaxToken.Colon(dummyTree, default), SyntaxToken.Identifier(dummyTree, default, string.Empty));
        var statement = new VariableDeclarationStatementSyntax(dummyTree, keyword, identifier, equals, expression, typeClause);
        AssertVisits(statement);
        statement = new (dummyTree, keyword, identifier, equals, expression, null);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_WhileStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.WhileKeyword(dummyTree, default);
        var condition = new LiteralExpressionSyntax(dummyTree, SyntaxToken.TrueKeyword(dummyTree, default));
        var body = new ExpressionStatementSyntax(dummyTree, condition);
        var statement = new WhileStatementSyntax(dummyTree, keyword, condition, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_DoWhileStatementSyntax_AcceptVisitsChildren()
    {
        var doKeyword = SyntaxToken.DoKeyword(dummyTree, default);
        var condition = new LiteralExpressionSyntax(dummyTree, SyntaxToken.TrueKeyword(dummyTree, default));
        var body = new ExpressionStatementSyntax(dummyTree, condition);
        var whileKeyword = SyntaxToken.WhileKeyword(dummyTree, default);
        var statement = new DoWhileStatementSyntax(dummyTree, doKeyword, body, whileKeyword, condition);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_ForStatementSyntax_AcceptVisitsChildren()
    {
        var forKeyword = SyntaxToken.ForKeyword(dummyTree, default);
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var equals = SyntaxToken.Equals(dummyTree, default);
        var dummy = SyntaxToken.TrueKeyword(dummyTree, default);
        var lowerBound = new LiteralExpressionSyntax(dummyTree, dummy);
        var toKeyWord = SyntaxToken.ToKeyword(dummyTree, default);
        var upperBound = new LiteralExpressionSyntax(dummyTree, dummy);
        var body = new ExpressionStatementSyntax(dummyTree, upperBound);
        var statement = new ForStatementSyntax(dummyTree, forKeyword, identifier, equals, lowerBound, toKeyWord, upperBound, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_ContinueStatementSyntax_AcceptVisitsChildren()
    {
        var continueKeyword = SyntaxToken.ContinueKeyword(dummyTree, default);
        var statement = new ContinueStatementSyntax(dummyTree, continueKeyword);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_BreakStatementSyntax_AcceptVisitsChildren()
    {
        var breakKeyword = SyntaxToken.BreakKeyword(dummyTree, default);
        var statement = new BreakStatementSyntax(dummyTree, breakKeyword);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_TypeClauseSyntax_AcceptVisitsChildren()
    {
        var colon = SyntaxToken.Colon(dummyTree, default);
        var identifier = SyntaxToken.Identifier(dummyTree, default, string.Empty);
        var typeClause = new TypeClauseSyntax(dummyTree, colon, identifier);
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
