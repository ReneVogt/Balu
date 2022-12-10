using System.Linq;
using Balu.Syntax;
using System.Reflection;
using Xunit;

namespace Balu.Tests.Syntax;

public sealed class ExpressionSyntaxTests
{
    [Fact]
    public void ExpressionSyntax_ProvidesFactoryForExpressions()
    {
        var expectedMethodNames = from type in typeof(ExpressionSyntax).Assembly.GetExportedTypes()
                                  where typeof(ExpressionSyntax).IsAssignableFrom(type) && type.IsPublic && !type.IsAbstract
                                  select type.Name[..^16];
        var actualMethodNames = from method in typeof(ExpressionSyntax).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where typeof(ExpressionSyntax).IsAssignableFrom(method.ReturnType)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }

}
