using System.Linq;
using System.Reflection;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.Syntax;
public sealed class SyntaxTokenTests
{
    [Fact]
    public void SyntaxToken_ProvidesFactoryForAllTokens()
    {
        var expectedMethodNames = from kind in typeof(SyntaxKind).GetEnumNames()
                                  where kind.EndsWith("Token") || kind.EndsWith("Keyword")
                                  select kind.EndsWith("Token") ? kind[..^5] : kind;
        var actualMethodNames = from method in typeof(SyntaxToken).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where method.ReturnType == typeof(SyntaxToken) &&
                                      method.GetParameters().FirstOrDefault()?.ParameterType == typeof(TextSpan)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }
}
