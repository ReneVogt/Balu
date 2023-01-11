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
        var expectedMethodNames = from SyntaxKind kind in typeof(SyntaxKind).GetEnumValues()
                                  where kind.IsToken()
                                  let s = kind.ToString()
                                  select kind.IsKeyword() ? s : kind.IsTrivia() ? s[..^6] : s[..^5];
        var actualMethodNames = from method in typeof(SyntaxToken).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                where method.ReturnType == typeof(SyntaxToken)
                                let parameters = method.GetParameters().Take(2).ToArray()
                                where parameters.Length == 2 && parameters[0].ParameterType == typeof(SyntaxTree) && parameters[1].ParameterType == typeof(TextSpan)
                                select method.Name;
        var missingMethods = expectedMethodNames.Except(actualMethodNames);
        Assert.Empty(missingMethods);
    }
}
