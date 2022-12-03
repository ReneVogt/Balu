using System.Collections.Generic;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// Represents a syntax token in Balu code.
/// </summary>
public sealed class SyntaxToken : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EndOfFileToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EndOfFileToken"/>.</returns>
    public static SyntaxToken EndOfFile(TextSpan textSpan) => new(SyntaxKind.EndOfFileToken, textSpan);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.</returns>
    public static SyntaxToken Bad(TextSpan textSpan, string text) => new(SyntaxKind.BadToken, textSpan, text);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.</returns>
    public static SyntaxToken WhiteSpace(TextSpan textSpan, string text) => new(SyntaxKind.WhiteSpaceToken, textSpan, text);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.
    /// </summary>
    /// <param name="value">The integer value of this number token.</param>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.</returns>
    public static SyntaxToken Number(int value, TextSpan textSpan, string text) => new(SyntaxKind.NumberToken, textSpan, text, value);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.</returns>
    public static SyntaxToken Plus(TextSpan textSpan) => new(SyntaxKind.PlusToken, textSpan, "+");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.</returns>
    public static SyntaxToken Minus(TextSpan textSpan) => new(SyntaxKind.MinusToken, textSpan, "-");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.</returns>
    public static SyntaxToken Star(TextSpan textSpan) => new(SyntaxKind.StarToken, textSpan, "*");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.</returns>
    public static SyntaxToken Slash(TextSpan textSpan) => new(SyntaxKind.SlashToken, textSpan, "/");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.</returns>
    public static SyntaxToken OpenParenthesis(TextSpan textSpan) => new(SyntaxKind.OpenParenthesisToken, textSpan, "(");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.</returns>
    public static SyntaxToken ClosedParenthesis(TextSpan textSpan) => new(SyntaxKind.ClosedParenthesisToken, textSpan, ")");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.</returns>
    public static SyntaxToken Equals(TextSpan textSpan) => new(SyntaxKind.EqualsToken, textSpan, "=");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.</returns>
    public static SyntaxToken Bang(TextSpan textSpan) => new(SyntaxKind.BangToken, textSpan, "!");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.</returns>
    public static SyntaxToken AmpersandAmpersand(TextSpan textSpan) => new(SyntaxKind.AmpersandAmpersandToken, textSpan, "&&");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.</returns>
    public static SyntaxToken PipePipe(TextSpan textSpan) => new(SyntaxKind.PipePipeToken, textSpan, "||");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.</returns>
    public static SyntaxToken EqualsEquals(TextSpan textSpan) => new(SyntaxKind.EqualsEqualsToken, textSpan, "==");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualToken"/>.</returns>
    public static SyntaxToken NotEquals(TextSpan textSpan) => new(SyntaxKind.BangEqualToken, textSpan, "!=");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IdentifierToken"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IdentifierToken"/>.</returns>
    public static SyntaxToken Identifier(TextSpan textSpan, string name) => new(SyntaxKind.IdentifierToken, textSpan, name);

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.TrueKeyword"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.TrueKeyword"/>.</returns>
    public static SyntaxToken TrueKeyword(TextSpan textSpan) => new(SyntaxKind.TrueKeyword, textSpan, "true");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.
    /// </summary>
    /// <param name="textSpan">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.</returns>
    public static SyntaxToken FalseKeyword(TextSpan textSpan) => new(SyntaxKind.FalseKeyword, textSpan, "false");

    /// <inheritdoc/>
    public override SyntaxKind Kind { get; }
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

    /// <summary>
    /// The original text in the input code.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// The <see cref="TextSpan"/> of this token in the input stream.
    /// </summary>
    public TextSpan TextSpan { get; }
    /// <summary>
    /// The value of this token, if there is any.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> with the given values.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of this token.</param>
    /// <param name="textSpan">The <see cref="TextSpan"/> of this token in the input stream.</param>
    /// <param name="text">The original text in the input code.</param>
    internal SyntaxToken(SyntaxKind kind, TextSpan textSpan = default, string text = "", object? value = null) => (Kind, Text, TextSpan, Value) = (kind, text, textSpan, value);

    internal override SyntaxNode Accept(SyntaxVisitor visitor) => this;

    /// <inheritdoc />
    public override string ToString() => $"{TextSpan}: {Kind} \"{Text}\" ({Value})";
}
