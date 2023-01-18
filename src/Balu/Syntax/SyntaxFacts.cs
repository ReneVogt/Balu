using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
#pragma warning disable CA1310

namespace Balu.Syntax;

public static class SyntaxFacts
{
    static readonly (string escaped, string unescaped)[] escapingCharacters = { ("r", "\r"), ("n", "\n"), ("t", "\t"), ("v", "\v"), ("\\", "\\"), ("\"", "\"") };
    static readonly Regex escapingRegex;
    public static ImmutableDictionary<string, string> EscapedToUnescapedCharacter { get; }
    public static ImmutableDictionary<string, string> UnescapedToEscapedCharacter { get; }

    [SuppressMessage("Performance", "CA1810:Statische Felder für Referenztyp inline initialisieren", Justification = "<Ausstehend>")]
    static SyntaxFacts()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        builder.AddRange(escapingCharacters.Select(pair => new KeyValuePair<string, string>(pair.escaped, pair.unescaped)));
        EscapedToUnescapedCharacter = builder.ToImmutable();

        builder.Clear();
        builder.AddRange(escapingCharacters.Select(pair => new KeyValuePair<string, string>(pair.unescaped, $"\\{pair.escaped}")));
        UnescapedToEscapedCharacter = builder.ToImmutable();


        escapingRegex = new (string.Join("|", escapingCharacters.Select(pair => Regex.Escape(pair.unescaped))), RegexOptions.Compiled);
    }

    public static bool IsKeyword(this SyntaxKind kind) => kind.ToString().EndsWith("Keyword");
    public static bool IsTrivia(this SyntaxKind kind) => kind.ToString().EndsWith("Trivia");
    public static bool IsComment(this SyntaxKind kind) => kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia;
    public static bool IsToken(this SyntaxKind kind) => !kind.IsTrivia() && (kind.ToString().EndsWith("Token")  || kind.IsKeyword());
    public static bool IsAssingmentToken(this SyntaxKind kind) => kind is SyntaxKind.EqualsToken or SyntaxKind.PlusEqualsToken
                                                                      or SyntaxKind.MinusEqualsToken or SyntaxKind.StarEqualsToken
                                                                      or SyntaxKind.SlashEqualsToken or SyntaxKind.AmpersandEqualsToken
                                                                      or SyntaxKind.PipeEqualsToken or SyntaxKind.CircumflexEqualsToken;

    public static SyntaxKind KeywordKind(this string literal) => literal switch
    {
        "else" => SyntaxKind.ElseKeyword,
        "for" => SyntaxKind.ForKeyword,
        "false" => SyntaxKind.FalseKeyword,
        "if" => SyntaxKind.IfKeyword,
        "let" => SyntaxKind.LetKeyword,
        "to" => SyntaxKind.ToKeyword,
        "true" => SyntaxKind.TrueKeyword,
        "var" => SyntaxKind.VarKeyword,
        "while" => SyntaxKind.WhileKeyword,
        "do" => SyntaxKind.DoKeyword,
        "continue" => SyntaxKind.ContinueKeyword,
        "break" => SyntaxKind.BreakKeyword,
        "function" => SyntaxKind.FunctionKeyword,
        "return" => SyntaxKind.ReturnKeyword,
        _ => SyntaxKind.IdentifierToken
    };

    public static int UnaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken or
            SyntaxKind.BangToken or 
            SyntaxKind.TildeToken => 100,
        _ => 0
    };
    public static int BinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.SlashToken or
            SyntaxKind.StarToken => 11,
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 10,

        SyntaxKind.EqualsEqualsToken or
            SyntaxKind.BangEqualsToken or
            SyntaxKind.GreaterToken or
            SyntaxKind.GreaterOrEqualsToken or
            SyntaxKind.LessToken or
            SyntaxKind.LessOrEqualsToken => 5,

        SyntaxKind.AmpersandToken or
            SyntaxKind.AmpersandAmpersandToken => 2,
        SyntaxKind.PipeToken or
            SyntaxKind.PipePipeToken or
            SyntaxKind.CircumflexToken => 1,
        _ => 0
    };

    public static string? GetText(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken => "+",
        SyntaxKind.MinusToken => "-",
        SyntaxKind.StarToken => "*",
        SyntaxKind.SlashToken => "/",
        SyntaxKind.OpenParenthesisToken => "(",
        SyntaxKind.ClosedParenthesisToken => ")",
        SyntaxKind.OpenBraceToken => "{",
        SyntaxKind.ClosedBraceToken => "}",
        SyntaxKind.EqualsToken => "=",
        SyntaxKind.PlusEqualsToken => "+=",
        SyntaxKind.MinusEqualsToken => "-=",
        SyntaxKind.StarEqualsToken => "*=",
        SyntaxKind.SlashEqualsToken => "/=",
        SyntaxKind.AmpersandEqualsToken => "&=",
        SyntaxKind.PipeEqualsToken => "|=",
        SyntaxKind.CircumflexEqualsToken => "^=",
        SyntaxKind.BangToken => "!",
        SyntaxKind.AmpersandToken => "&",
        SyntaxKind.AmpersandAmpersandToken => "&&",
        SyntaxKind.PipeToken => "|",
        SyntaxKind.PipePipeToken => "||",
        SyntaxKind.CircumflexToken => "^",
        SyntaxKind.TildeToken => "~",
        SyntaxKind.EqualsEqualsToken => "==",
        SyntaxKind.BangEqualsToken => "!=",
        SyntaxKind.GreaterToken => ">",
        SyntaxKind.GreaterOrEqualsToken => ">=",
        SyntaxKind.LessToken => "<",
        SyntaxKind.LessOrEqualsToken => "<=",
        SyntaxKind.CommaToken => ",",
        SyntaxKind.ColonToken => ":",
        SyntaxKind.TrueKeyword => "true",
        SyntaxKind.FalseKeyword => "false",
        SyntaxKind.LetKeyword => "let",
        SyntaxKind.VarKeyword => "var",
        SyntaxKind.IfKeyword => "if",
        SyntaxKind.ElseKeyword => "else",
        SyntaxKind.WhileKeyword => "while",
        SyntaxKind.DoKeyword => "do",
        SyntaxKind.ForKeyword => "for",
        SyntaxKind.ToKeyword => "to",
        SyntaxKind.ContinueKeyword => "continue",
        SyntaxKind.BreakKeyword => "break",
        SyntaxKind.FunctionKeyword => "function",
        SyntaxKind.ReturnKeyword => "return",
        _ => null
    };

    public static IEnumerable<SyntaxKind> GetUnaryOperators() => from kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                                                                 where kind.UnaryOperatorPrecedence() > 0
                                                                 select kind;
    public static IEnumerable<SyntaxKind> GetBinaryOperators() => from kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                                                                  where kind.BinaryOperatorPrecedence() > 0
                                                                  select kind;

    public static string EscapeString(this string unescaped) =>
        escapingRegex.Replace(unescaped, match => UnescapedToEscapedCharacter[match.Value]);
}
