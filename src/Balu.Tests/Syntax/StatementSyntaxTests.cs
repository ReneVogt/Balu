using System.Linq;
using Balu.Syntax;
using System.Reflection;
using Xunit;

namespace Balu.Tests.Syntax;

public sealed class StatementSyntaxTests
{
    [Fact]
    public void StatementSyntax_ProvidesFactoryForStatements()
    {
        var expectedMethodNames = from type in typeof(StatementSyntax).Assembly.GetExportedTypes()
                                  where typeof(StatementSyntax).IsAssignableFrom(type) && type.IsPublic && !type.IsAbstract && type != typeof(ElseClauseSyntax)
                                  select type.Name[..^6];
        var actualMethodNames = from method in typeof(StatementSyntax).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where typeof(StatementSyntax).IsAssignableFrom(method.ReturnType)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }

}
