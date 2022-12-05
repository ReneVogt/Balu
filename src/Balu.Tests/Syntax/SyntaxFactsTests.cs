using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.Syntax;

public class SyntaxFactsTests
{
    [Theory]
    [MemberData(nameof(GetSyntaxKinds))]
    public void SyntaxFacts_GetText_RoundTrips(SyntaxKind kind)
    {
        var text = kind.GetText();
        if (text is null) return;
        var token = Assert.Single(SyntaxTree.ParseTokens(text));
        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    public static IEnumerable<object[]> GetSyntaxKinds() =>
        Enum.GetValues(typeof(SyntaxKind)).Cast<object>().Select(enumValue => new[] { enumValue });
}
