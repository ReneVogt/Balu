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
    public static SyntaxToken Number(TextSpan span, int value, string text) => new(SyntaxKind.NumberToken, span, text ?? throw new ArgumentNullException(nameof(text)), value);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.</returns>
    public static SyntaxToken Plus(TextSpan span) => new(SyntaxKind.PlusToken, span, SyntaxKind.PlusToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.</returns>
    public static SyntaxToken Minus(TextSpan span) => new(SyntaxKind.MinusToken, span, SyntaxKind.MinusToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.</returns>
    public static SyntaxToken Star(TextSpan span) => new(SyntaxKind.StarToken, span, SyntaxKind.StarToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.</returns>
    public static SyntaxToken Slash(TextSpan span) => new(SyntaxKind.SlashToken, span, SyntaxKind.SlashToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.</returns>
    public static SyntaxToken OpenParenthesis(TextSpan span) => new(SyntaxKind.OpenParenthesisToken, span, SyntaxKind.OpenParenthesisToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.</returns>
    public static SyntaxToken ClosedParenthesis(TextSpan span) => new(SyntaxKind.ClosedParenthesisToken, span, SyntaxKind.ClosedParenthesisToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenBraceToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenBraceToken"/>.</returns>
    public static SyntaxToken OpenBrace(TextSpan span) => new(SyntaxKind.OpenBraceToken, span, SyntaxKind.OpenBraceToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedBraceToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedBraceToken"/>.</returns>
    public static SyntaxToken ClosedBrace(TextSpan span) => new(SyntaxKind.ClosedBraceToken, span, SyntaxKind.ClosedBraceToken.GetText()!);

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsToken"/>.</returns>
    public static SyntaxToken Equals(TextSpan span) => new(SyntaxKind.EqualsToken, span, SyntaxKind.EqualsToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangToken"/>.</returns>
    public static SyntaxToken Bang(TextSpan span) => new(SyntaxKind.BangToken, span, SyntaxKind.BangToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.AmpersandAmpersandToken"/>.</returns>
    public static SyntaxToken AmpersandAmpersand(TextSpan span) => new(SyntaxKind.AmpersandAmpersandToken, span, SyntaxKind.AmpersandAmpersandToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PipePipeToken"/>.</returns>
    public static SyntaxToken PipePipe(TextSpan span) => new(SyntaxKind.PipePipeToken, span, SyntaxKind.PipePipeToken.GetText()!);

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EqualsEqualsToken"/>.</returns>
    public static SyntaxToken EqualsEquals(TextSpan span) => new(SyntaxKind.EqualsEqualsToken, span, SyntaxKind.EqualsEqualsToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BangEqualsToken"/>.</returns>
    public static SyntaxToken BangEquals(TextSpan span) => new(SyntaxKind.BangEqualsToken, span, SyntaxKind.BangEqualsToken.GetText()!);

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LessToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LessToken"/>.</returns>
    public static SyntaxToken Less(TextSpan span) => new(SyntaxKind.LessToken, span, SyntaxKind.LessToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LessOrEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LessOrEqualsToken"/>.</returns>
    public static SyntaxToken LessOrEquals(TextSpan span) => new(SyntaxKind.LessOrEqualsToken, span, SyntaxKind.LessOrEqualsToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.GreaterToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.GreaterToken"/>.</returns>
    public static SyntaxToken Greater(TextSpan span) => new(SyntaxKind.GreaterToken, span, SyntaxKind.GreaterToken.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.GreaterOrEqualsToken"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.GreaterOrEqualsToken"/>.</returns>
    public static SyntaxToken GreaterOrEquals(TextSpan span) => new(SyntaxKind.GreaterOrEqualsToken, span, SyntaxKind.GreaterOrEqualsToken.GetText()!);
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
    public static SyntaxToken TrueKeyword(TextSpan span) => new(SyntaxKind.TrueKeyword, span, SyntaxKind.TrueKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.FalseKeyword"/>.</returns>
    public static SyntaxToken FalseKeyword(TextSpan span) => new(SyntaxKind.FalseKeyword, span, SyntaxKind.FalseKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LetKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.LetKeyword"/>.</returns>
    public static SyntaxToken LetKeyword(TextSpan span) => new(SyntaxKind.LetKeyword, span, SyntaxKind.LetKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.VarKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.VarKeyword"/>.</returns>
    public static SyntaxToken VarKeyword(TextSpan span) => new(SyntaxKind.VarKeyword, span, SyntaxKind.VarKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IfKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.IfKeyword"/>.</returns>
    public static SyntaxToken IfKeyword(TextSpan span) => new(SyntaxKind.IfKeyword, span, SyntaxKind.IfKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ElseKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ElseKeyword"/>.</returns>
    public static SyntaxToken ElseKeyword(TextSpan span) => new(SyntaxKind.ElseKeyword, span, SyntaxKind.ElseKeyword.GetText()!);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhileKeyword"/>.
    /// </summary>
    /// <param name="span">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhileKeyword"/>.</returns>
    public static SyntaxToken WhileKeyword(TextSpan span) => new(SyntaxKind.WhileKeyword, span, SyntaxKind.WhileKeyword.GetText()!);
}
