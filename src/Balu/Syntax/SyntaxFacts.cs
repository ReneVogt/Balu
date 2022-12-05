﻿namespace Balu.Syntax;

/// <summary>
/// Provides methods to evalute syntax facts.
/// </summary>
public static class SyntaxFacts
{
    /// <summary>
    /// Determines the precedence of an unary operator.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of the unary operator.</param>
    /// <returns>The precedence of the given operator.</returns>
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
    /// <returns>The precedence of the given operator.</returns>
    public static int BinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.SlashToken or
            SyntaxKind.StarToken => 11,
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 10,

        SyntaxKind.EqualsEqualsToken or
            SyntaxKind.BangEqualToken => 5,

        SyntaxKind.AmpersandAmpersandToken => 2,
        SyntaxKind.PipePipeToken => 1,
        _ => 0
    };
    /// <summary>
    /// Determines the <see cref="SyntaxKind"/> of a given keyword.
    /// </summary>
    /// <param name="literal">The string representing the keyword.</param>
    /// <returns>The <see cref="SyntaxKind"/> of the given keyword, or <see cref="SyntaxKind.IdentifierToken"/> if it's not a keyword.</returns>
    public static SyntaxKind KeywordKind(this string literal) => literal switch
    {
        "true" => SyntaxKind.TrueKeyword,
        "false" => SyntaxKind.FalseKeyword,
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
        SyntaxKind.EqualsToken => "=",
        SyntaxKind.BangToken => "!",
        SyntaxKind.AmpersandAmpersandToken => "&&",
        SyntaxKind.PipePipeToken => "||",
        SyntaxKind.EqualsEqualsToken => "==",
        SyntaxKind.BangEqualToken => "!=",
        SyntaxKind.TrueKeyword => "true",
        SyntaxKind.FalseKeyword => "false",
        _ => null
    };
}
