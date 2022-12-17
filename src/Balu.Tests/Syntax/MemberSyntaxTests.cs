using System.Linq;
using System.Reflection;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax;
public sealed class MemberSyntaxTests
{
    [Fact]
    public void MemberSyntax_ProvidesFactoryForDirectlyDerivedNodes()
    {
        var expectedMethodNames = from type in typeof(MemberSyntax).Assembly.GetExportedTypes()
                                  where type.BaseType == typeof(MemberSyntax) &&
                                        type.IsPublic && !type.IsAbstract
                                  select type.Name[..^6];
        var actualMethodNames = from method in typeof(MemberSyntax).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where typeof(MemberSyntax).IsAssignableFrom(method.ReturnType)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }
}
