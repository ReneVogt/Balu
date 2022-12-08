using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

/// <summary>
/// Represents a syntax token in Balu code.
/// </summary>
public sealed class SyntaxToken : SyntaxNode
{
    /// <inheritdoc/>
    public override SyntaxKind Kind { get; }
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

    /// <summary>
    /// The original text in the input code.
    /// </summary>
    public string Text { get; }
    /// <inheritdoc/>
    public override TextSpan Span { get; }
    /// <summary>
    /// The value of this token, if there is any.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> with the given values.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of this token.</param>
    /// <param name="span">The <see cref="Span"/> of this token in the input stream.</param>
    /// <param name="text">The original text in the input code.</param>
    internal SyntaxToken(SyntaxKind kind, TextSpan span = default, string text = "", object? value = null) => (Kind, Text, Span, Value) = (kind, text, span, value);

    internal override SyntaxNode Accept(SyntaxVisitor visitor) => this;

    /// <inheritdoc />
    public override string ToString() => $"{Span}: {Kind} \"{Text}\" ({Value})";

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EndOfFileToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EndOfFileToken"/>.</returns>
    public static SyntaxToken EndOfFile(TextSpan span) => new(SyntaxKind.EndOfFileToken, span);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.</returns>
    public static SyntaxToken Bad(TextSpan span, string text) => new(SyntaxKind.BadToken, span, text);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    public static SyntaxToken WhiteSpace(TextSpan span, string text) => new(SyntaxKind.WhiteSpaceToken, span, text ?? throw new ArgumentNullException(nameof(text)));
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.
    /// </summary>
    /// <param name="value">The integer value of this number token.</param>
    /// <param name="span">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    public static SyntaxToken Number(int value, TextSpan span, string text) => new(SyntaxKind.NumberToken, span, text ?? throw new ArgumentNullException(nameof(text)), value);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.</returns>
    public static SyntaxToken Plus(TextSpan span) => new(SyntaxKind.PlusToken, span, "+");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.</returns>
    public static SyntaxToken Minus(TextSpan span) => new(SyntaxKind.MinusToken, span, "-");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.</returns>
    public static SyntaxToken Star(TextSpan span) => new(SyntaxKind.StarToken, span, "*");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.</returns>
    public static SyntaxToken Slash(TextSpan span) => new(SyntaxKind.SlashToken, span, "/");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.</returns>
    public static SyntaxToken OpenParenthesis(TextSpan span) => new(SyntaxKind.OpenParenthesisToken, span, "(");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.</returns>
    public static SyntaxToken ClosedParenthesis(TextSpan span) => new(SyntaxKind.ClosedParenthesisToken, span, ")");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.</returns>
    public static SyntaxToken Equals(TextSpan span) => new(SyntaxKind.EqualsToken, span, "=");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.</returns>
    public static SyntaxToken Bang(TextSpan span) => new(SyntaxKind.BangToken, span, "!");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.</returns>
    public static SyntaxToken AmpersandAmpersand(TextSpan span) => new(SyntaxKind.AmpersandAmpersandToken, span, "&&");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.</returns>
    public static SyntaxToken PipePipe(TextSpan span) => new(SyntaxKind.PipePipeToken, span, "||");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.</returns>
    public static SyntaxToken EqualsEquals(TextSpan span) => new(SyntaxKind.EqualsEqualsToken, span, "==");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualsToken"/>.</returns>
    public static SyntaxToken NotEquals(TextSpan span) => new(SyntaxKind.BangEqualsToken, span, "!=");

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IdentifierToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IdentifierToken"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
    public static SyntaxToken Identifier(TextSpan span, string name) => new(SyntaxKind.IdentifierToken, span, name ?? throw new ArgumentNullException(nameof(name)));

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.TrueKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.TrueKeyword"/>.</returns>
    public static SyntaxToken TrueKeyword(TextSpan span) => new(SyntaxKind.TrueKeyword, span, "true");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.</returns>
    public static SyntaxToken FalseKeyword(TextSpan span) => new(SyntaxKind.FalseKeyword, span, "false");
}
