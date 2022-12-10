using System;
using System.Collections.Generic;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// Provides methods to evalute syntax facts.
/// </summary>
public static class SyntaxFacts
{
    /// <summary>
    /// Determines the precedence of an unary operator.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of the unary operator.</param>
    /// <returns>The precedence of the given operator or 0 if it's not a unary operator.</returns>
    public static int UnaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 100,
        SyntaxKind.BangToken => 100,
        _ => 0
    };
    /// <summary>
    /// Determines the precedence of a binary operator.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of the binary operator.</param>
    /// <returns>The precedence of the given operator or 0 if it's not a binary operator..</returns>
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
    /// <summary>
    /// Determines the <see cref="SyntaxKind"/> of a given keyword.
    /// </summary>
    /// <param name="literal">The string representing the keyword.</param>
    /// <returns>The <see cref="SyntaxKind"/> of the given keyword, or <see cref="SyntaxKind.IdentifierToken"/> if it's not a keyword.</returns>
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
        _ => SyntaxKind.IdentifierToken
    };

    /// <summary>
    /// Gets the text a given <see cref="SyntaxKind"/> should have in Balu code.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> to translate.</param>
    /// <returns>A string representing the <paramref name="kind"/>.</returns>
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
        SyntaxKind.BangToken => "!",
        SyntaxKind.AmpersandToken => "&",
        SyntaxKind.AmpersandAmpersandToken => "&&",
        SyntaxKind.PipeToken => "|",
        SyntaxKind.PipePipeToken => "||",
        SyntaxKind.CircumflexToken => "^",
        SyntaxKind.EqualsEqualsToken => "==",
        SyntaxKind.BangEqualsToken => "!=",
        SyntaxKind.GreaterToken => ">",
        SyntaxKind.GreaterOrEqualsToken => ">=",
        SyntaxKind.LessToken => "<",
        SyntaxKind.LessOrEqualsToken => "<=",
        SyntaxKind.TrueKeyword => "true",
        SyntaxKind.FalseKeyword => "false",
        SyntaxKind.LetKeyword => "let",
        SyntaxKind.VarKeyword => "var",
        SyntaxKind.IfKeyword => "if",
        SyntaxKind.ElseKeyword => "else",
        SyntaxKind.WhileKeyword => "while",
        SyntaxKind.ForKeyword => "for",
        SyntaxKind.ToKeyword => "to",
        _ => null
    };

    /// <summary>
    /// Returns a sequence of all unary Balu operators.
    /// </summary>
    /// <returns>A sequence of all unary Balu operators.</returns>
    public static IEnumerable<SyntaxKind> GetUnaryOperators() => from kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                                                                 where kind.UnaryOperatorPrecedence() > 0
                                                                 select kind;
    /// <summary>
    /// Returns a sequence of all binary Balu operators.
    /// </summary>
    /// <returns>A sequence of all binary Balu operators.</returns>
    public static IEnumerable<SyntaxKind> GetBinaryOperators() => from kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                                                                  where kind.BinaryOperatorPrecedence() > 0
                                                                  select kind;


}
