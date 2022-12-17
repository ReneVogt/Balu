using System.Linq;
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
                                  where type.BaseType == typeof(SyntaxNode) &&
                                        type != typeof(SyntaxToken) &&
                                        type.IsPublic && !type.IsAbstract
                                  select type.Name.EndsWith("ClauseSyntax") ? type.Name[..^12] : type.Name[..^6];
        var actualMethodNames = from method in typeof(SyntaxNode).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where typeof(SyntaxNode).IsAssignableFrom(method.ReturnType)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }

}
