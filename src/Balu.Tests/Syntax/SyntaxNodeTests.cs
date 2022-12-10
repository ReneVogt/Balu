﻿using System.Linq;
using Balu.Syntax;
using System.Reflection;
using Xunit;

namespace Balu.Tests.Syntax;

public sealed class SyntaxNodeTests
{
    [Fact]
    public void SyntaxNode_ProvidesFactoryForDirectlyDerivedNodes()
    {
        var expectedMethodNames = from type in typeof(SyntaxNode).Assembly.GetExportedTypes()
                                  where typeof(SyntaxNode).IsAssignableFrom(type) && 
                                        !typeof(ExpressionSyntax).IsAssignableFrom(type) &&
                                        !typeof(StatementSyntax).IsAssignableFrom(type) &&
                                        type != typeof(SyntaxToken) &&
                                        type.IsPublic && !type.IsAbstract && type != typeof(ElseClauseSyntax)
                                  select type.Name[..^6];
        var actualMethodNames = from method in typeof(SyntaxNode).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where typeof(SyntaxNode).IsAssignableFrom(method.ReturnType)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }

}
