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
        var literal = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty));
        var equalsToken = SyntaxToken.Equals(null, default);
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var assignment = new AssignmentExpressionSyntax(null, identifier, equalsToken, literal);
        AssertVisits(assignment);
    }
    [Fact]
    public void SyntaxVisitor_CallExpressionSyntax_AcceptVisitsChildren()
    {
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var open = SyntaxToken.OpenParenthesis(null, default);
        var parameter = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(null, default);
        var call = new CallExpressionSyntax(null, identifier, open, new (new SyntaxNode[] { parameter }.ToImmutableArray()), close);
        AssertVisits(call);
    }
    [Fact]
    public void SyntaxVisitor_BinaryExpressionSyntax_AcceptVisitsChildren()
    {
        var left = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty));
        var operatorToken = SyntaxToken.Plus(null, default);
        var right = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty));
        var binary = new BinaryExpressionSyntax(null, left, operatorToken, right);
        AssertVisits(binary);
    }
    [Fact]
    public void SyntaxVisitor_BlockStatementSyntax_AcceptVisitsChildren()
    {
        var openBraceToken = SyntaxToken.OpenBrace(null, default);
        var statements = Enumerable.Range(0, 5)
                                   .Select(_ => new ExpressionStatementSyntax(null,
                                                                              new NameExpressionSyntax(
                                                                                  null, SyntaxToken.Identifier(null, default, string.Empty))));
        var closedBraceToken = SyntaxToken.ClosedBrace(null, default);
        var statement = new BlockStatementSyntax(null, openBraceToken, statements.ToImmutableArray<StatementSyntax>(), closedBraceToken);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_CompilationUnitSyntax_AcceptVisitsChildren()
    {
        var statement = new ExpressionStatementSyntax(null, new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty)));
        var eof = SyntaxToken.EndOfFile(null, default);
        var compilationUnit = new CompilationUnitSyntax(null, new MemberSyntax[]{new GlobalStatementSyntax(null, statement) }.ToImmutableArray(), eof);
        AssertVisits(compilationUnit);
    }
    [Fact]
    public void SyntaxVisitor_GlobalStatementSyntax_AcceptVisitsChildren()
    {
        var statement = new ExpressionStatementSyntax(null, new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty)));
        var globalStatement = new GlobalStatementSyntax(null, statement);
        AssertVisits(globalStatement);
    }
    [Fact]
    public void SyntaxVisitor_FunctionDeclarationSyntax_AcceptVisitsChildren()
    {
        var functionKeyword = SyntaxToken.FunctionKeyword(null, default);
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var openParenthesis = SyntaxToken.OpenParenthesis(null, default);
        var type = new TypeClauseSyntax(null, identifier, identifier);
        var parameters =
            new SeparatedSyntaxList<ParameterSyntax>(new SyntaxNode[] { new ParameterSyntax(null, identifier, type) }
                                                         .ToImmutableArray());
        var closeParenthesis = SyntaxToken.OpenParenthesis(null, default);
        var body = new BlockStatementSyntax(null, SyntaxToken.OpenBrace(null, default), ImmutableArray<StatementSyntax>.Empty, SyntaxToken.ClosedBrace(null, default));
        var functionDelcaration = new FunctionDeclarationSyntax(null, functionKeyword, identifier, openParenthesis, parameters, closeParenthesis, type, body);
        AssertVisits(functionDelcaration);
    }
    [Fact]
    public void SyntaxVisitor_ReturnStatementSyntax_AcceptVisitsChildren()
    {
        var returnKeyword = SyntaxToken.ReturnKeyword(null, default);
        var expression = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, "0"));
        var returnStatement = new ReturnStatementSyntax(null, returnKeyword, expression);
        AssertVisits(returnStatement);
    }
    [Fact]
    public void SyntaxVisitor_ParameterSyntax_AcceptVisitsChildren()
    {
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var type = new TypeClauseSyntax(null, SyntaxToken.Colon(null, default), SyntaxToken.Identifier(null, default, string.Empty));
        var parameter = new ParameterSyntax(null, identifier, type);
        AssertVisits(parameter);
    }
    [Fact]
    public void SyntaxVisitor_ElseClauseSyntax_AcceptVisitsChildren()
    {
        var elseToken = SyntaxToken.ElseKeyword(null, default);
        var statement = new ExpressionStatementSyntax(null, new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty)));
        var elseClause = new ElseClauseSyntax(null, elseToken, statement);
        AssertVisits(elseClause);
    }
    [Fact]
    public void SyntaxVisitor_ExpressionStatementSyntax_AcceptVisitsChildren()
    {
        var expression = new LiteralExpressionSyntax(null, SyntaxToken.Number(null, default, 0, string.Empty));
        var statement = new ExpressionStatementSyntax(null, expression);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_IfStatementSyntax_AcceptVisitsChildren()
    {
        var ifToken = SyntaxToken.IfKeyword(null, default);
        var condition = new LiteralExpressionSyntax(null, SyntaxToken.TrueKeyword(null, default));
        var thenStatement = new ExpressionStatementSyntax(null, condition);
        var elseToken = SyntaxToken.ElseKeyword(null, default);
        var elseStatement = new ExpressionStatementSyntax(null, condition);
        var elseClause = new ElseClauseSyntax(null, elseToken, elseStatement);
        var statement = new IfStatementSyntax(null, ifToken, condition, thenStatement, elseClause);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_LiteralExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(null, default, string.Empty);
        var expression = new LiteralExpressionSyntax(null, token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_NameExpressionSyntax_AcceptVisitsChildren()
    {
        var token = SyntaxToken.Identifier(null, default, string.Empty);
        var expression = new NameExpressionSyntax(null, token);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_ParenthesizedExpressionSyntax_AcceptVisitsChildren()
    {
        var open = SyntaxToken.OpenParenthesis(null, default);
        var inner = new NameExpressionSyntax(null, SyntaxToken.Identifier(null, default, string.Empty));
        var close = SyntaxToken.ClosedParenthesis(null, default);
        var expression = new ParenthesizedExpressionSyntax(null, open, inner, close);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_UnaryExpressionSyntax_AcceptVisitsChildren()
    {
        var operatorToken = SyntaxToken.Bang(null, default);
        var operand = new LiteralExpressionSyntax(null, SyntaxToken.TrueKeyword(null, default));
        var expression = new UnaryExpressionSyntax(null, operatorToken, operand);
        AssertVisits(expression);
    }
    [Fact]
    public void SyntaxVisitor_VariableDeclarationStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.LetKeyword(null, default);
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var equals = SyntaxToken.Equals(null, default);
        var expression = new LiteralExpressionSyntax(null, SyntaxToken.TrueKeyword(null, default));
        var typeClause = new TypeClauseSyntax(null, SyntaxToken.Colon(null, default), SyntaxToken.Identifier(null, default, string.Empty));
        var statement = new VariableDeclarationStatementSyntax(null, keyword, identifier, equals, expression, typeClause);
        AssertVisits(statement);
        statement = new (null, keyword, identifier, equals, expression, null);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_WhileStatementSyntax_AcceptVisitsChildren()
    {
        var keyword = SyntaxToken.WhileKeyword(null, default);
        var condition = new LiteralExpressionSyntax(null, SyntaxToken.TrueKeyword(null, default));
        var body = new ExpressionStatementSyntax(null, condition);
        var statement = new WhileStatementSyntax(null, keyword, condition, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_DoWhileStatementSyntax_AcceptVisitsChildren()
    {
        var doKeyword = SyntaxToken.DoKeyword(null, default);
        var condition = new LiteralExpressionSyntax(null, SyntaxToken.TrueKeyword(null, default));
        var body = new ExpressionStatementSyntax(null, condition);
        var whileKeyword = SyntaxToken.WhileKeyword(null, default);
        var statement = new DoWhileStatementSyntax(null, doKeyword, body, whileKeyword, condition);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_ForStatementSyntax_AcceptVisitsChildren()
    {
        var forKeyword = SyntaxToken.ForKeyword(null, default);
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var equals = SyntaxToken.Equals(null, default);
        var dummy = SyntaxToken.TrueKeyword(null, default);
        var lowerBound = new LiteralExpressionSyntax(null, dummy);
        var toKeyWord = SyntaxToken.ToKeyword(null, default);
        var upperBound = new LiteralExpressionSyntax(null, dummy);
        var body = new ExpressionStatementSyntax(null, upperBound);
        var statement = new ForStatementSyntax(null, forKeyword, identifier, equals, lowerBound, toKeyWord, upperBound, body);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_ContinueStatementSyntax_AcceptVisitsChildren()
    {
        var continueKeyword = SyntaxToken.ContinueKeyword(null, default);
        var statement = new ContinueStatementSyntax(null, continueKeyword);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_BreakStatementSyntax_AcceptVisitsChildren()
    {
        var breakKeyword = SyntaxToken.BreakKeyword(null, default);
        var statement = new BreakStatementSyntax(null, breakKeyword);
        AssertVisits(statement);
    }
    [Fact]
    public void SyntaxVisitor_TypeClauseSyntax_AcceptVisitsChildren()
    {
        var colon = SyntaxToken.Colon(null, default);
        var identifier = SyntaxToken.Identifier(null, default, string.Empty);
        var typeClause = new TypeClauseSyntax(null, colon, identifier);
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
