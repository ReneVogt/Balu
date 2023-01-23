using Balu.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Balu.Tests.Syntax.LexerTests;

public partial class LexerTests
{
    public static IEnumerable<object[]> ProvideSingleTokens() => GetSingleTokens().Select(x => new object[] { x.text, x.kind });
    public static IEnumerable<(string text, SyntaxKind kind)> GetSingleTokens() =>
        Enum.GetValues(typeof(SyntaxKind))
            .Cast<SyntaxKind>()
            .Select(kind => (text: kind.GetText()!, kind))
            .Where(x => !string.IsNullOrEmpty(x.text))
            .Concat(
                new (string text, SyntaxKind kind)[]
                {
                    ("01", kind: SyntaxKind.NumberToken),
                    ("123", kind: SyntaxKind.NumberToken),
                    ("987654321", kind: SyntaxKind.NumberToken),
                    ("myNameIs", kind: SyntaxKind.IdentifierToken),
                    ("_my123", kind: SyntaxKind.IdentifierToken),
                    ("my_123_NameIs", kind: SyntaxKind.IdentifierToken),
                    ("x", kind: SyntaxKind.IdentifierToken),
                    ("true", kind: SyntaxKind.TrueKeyword),
                    ("false", kind: SyntaxKind.FalseKeyword),
                    ("\"Escaped\\\"String with even \\r and \\n, \\t and \\v\"", SyntaxKind.StringToken)
                });
    static IEnumerable<(string text, SyntaxKind kind)> GetSeparators() => new (string text, SyntaxKind kind)[]
    {
        (" ", kind: SyntaxKind.WhiteSpaceTrivia),
        ("  ", kind: SyntaxKind.WhiteSpaceTrivia),
        ("\r\n", kind: SyntaxKind.LineBreakTrivia),
        ("\t\v", kind: SyntaxKind.WhiteSpaceTrivia),
        ("// single \r\n", kind: SyntaxKind.SingleLineCommentTrivia),
        ("/* multi\r\nline*/", kind: SyntaxKind.MultiLineCommentTrivia)
    };

    public static IEnumerable<object[]> ProvideTokenPairs() => from left in GetSingleTokens()
                                                               from right in GetSingleTokens()
                                                               where (left.kind, right.kind) switch
                                                               {
                                                                   (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => false,
                                                                   (_, SyntaxKind.NumberToken) => left.kind != SyntaxKind.IdentifierToken && !left.kind.IsKeyword(),
                                                                   (SyntaxKind.IdentifierToken, _) => right.kind != SyntaxKind.IdentifierToken && !right.kind.IsKeyword(),
                                                                   (_, SyntaxKind.IdentifierToken) => left.kind != SyntaxKind.IdentifierToken && !left.kind.IsKeyword(),
                                                                   (SyntaxKind.BangToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.BangToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.GreaterToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.GreaterToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.LessToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.LessToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandEqualsToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.PipeToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.PipeEqualsToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.SlashToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.StarToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.StarEqualsToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.SlashEqualsToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.MultiLineCommentTrivia) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.SingleLineCommentTrivia) => false,
                                                                   (SyntaxKind.PlusToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.PlusToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.PlusToken, SyntaxKind.PlusToken) => false,
                                                                   (SyntaxKind.PlusToken, SyntaxKind.PlusPlusToken) => false,
                                                                   (SyntaxKind.PlusToken, SyntaxKind.PlusEqualsToken) => false,
                                                                   (SyntaxKind.MinusToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.MinusToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.MinusToken, SyntaxKind.MinusToken) => false,
                                                                   (SyntaxKind.MinusToken, SyntaxKind.MinusMinusToken) => false,
                                                                   (SyntaxKind.MinusToken, SyntaxKind.MinusEqualsToken) => false,
                                                                   (SyntaxKind.StarToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.StarToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.SlashToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.AmpersandToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.PipeToken, SyntaxKind.EqualsToken) => false,
                                                                   (SyntaxKind.CircumflexToken, SyntaxKind.EqualsEqualsToken) => false,
                                                                   (SyntaxKind.CircumflexToken, SyntaxKind.EqualsToken) => false,
                                                                   _ => !(left.kind.ToString().EndsWith("Keyword") && right.kind.ToString().EndsWith("Keyword"))
                                                               }
                                                               select new object[] { left.text, left.kind, right.text, right.kind };

    public static IEnumerable<object[]> ProvideSeparatedTokenPairs() => from left in GetSingleTokens()
                                                                        from right in GetSingleTokens()
                                                                        from separator in GetSeparators()
                                                                        where (left.kind, separator.kind, right.kind) switch
                                                                        {
                                                                            (SyntaxKind.SlashToken, SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia, _) => false,
                                                                            _ => true
                                                                        }
                                                                        select new object[]
                                                                        {
                                                                            left.text, left.kind, separator.text, separator.kind, right.text,
                                                                            right.kind
                                                                        };
}
