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

    [Theory]
    [InlineData("\r\n\v\\\t\"", "\\r\\n\\v\\\\\\t\\\"")]
    [InlineData("a\rb\nc\vd\te\\f\"g", "a\\rb\\nc\\vd\\te\\\\f\\\"g")]
    public void SyntaxFacts_EscapeString_EscapesCorrectly(string input, string expected) => Assert.Equal(expected, input.EscapeString());

    public static IEnumerable<object[]> GetSyntaxKinds() =>
        Enum.GetValues(typeof(SyntaxKind)).Cast<object>().Select(enumValue => new[] { enumValue });
}
