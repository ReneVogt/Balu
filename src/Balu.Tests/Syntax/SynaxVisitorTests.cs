using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax;

public class SynaxVisitorTests
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
                                                var expectedMethodName = $"Visit{type.Name.Substring(0, type.Name.Length - 6)}"; // remove "Syntax"
                                                return method.Name == expectedMethodName && method.GetParameters()[0].ParameterType == type;

                                            })
                                        )
                                        .ToList();
        Assert.Empty(missingTypes);

    }
    [Fact]
    public void SyntaxVisitor_AcceptAndGetChildrenTestedForAllSyntaxNodes()
    {
        var testedNames = from method in typeof(SynaxVisitorTests).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          let match = testingMethodRegex.Match(method.Name)
                          where match.Success && match.Groups.Count == 2 && method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.GetCustomAttribute<FactAttribute>() is not null
                          select match.Groups[1].Value;

        var expectedNames = GetAllSyntaxNodeTypes().Select(type => type.Name);
        var missingNames = expectedNames.Except(testedNames).ToList();
        Assert.Empty(missingNames);
    }

    static readonly Regex visitMethodRegex = new("Visit(.*?)", RegexOptions.Compiled);
    static readonly Regex testingMethodRegex = new("SyntaxVisitor_(.*?)_AcceptVisitsChildren", RegexOptions.Compiled);
    static IEnumerable<Type> GetAllSyntaxNodeTypes() => from type in typeof(SyntaxNode).Assembly.GetExportedTypes()
                                                        where IsDerivedFromSyntaxNode(type) && type.IsPublic && !type.IsAbstract
                                                        select type;

    [Fact]
    public void SyntaxVisitor_AssignmentExpressionSyntax_AcceptVisitsChildren()
    {
        var literal = ExpressionSyntax.Literal(SyntaxToken.Number(0, default, string.Empty));
        var equalsToken = SyntaxToken.Equals(default);
        var identifier = SyntaxToken.Identifier(default, string.Empty);
        var assignment = ExpressionSyntax.Assignment(identifier, equalsToken, literal);
        AssertVisits(assignment);
    }

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
    static bool IsDerivedFromSyntaxNode(Type t)
    {
        if (t == typeof(SyntaxNode)) return true;
        while (t.BaseType is not null)
        {
            t = t.BaseType;
            if (t == typeof(SyntaxNode)) return true;
        }

        return false;
    }
}
