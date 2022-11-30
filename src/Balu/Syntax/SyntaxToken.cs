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
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.EndOfFileToken"/>.</returns>
    public static SyntaxToken EndOfFile(int position) => new(SyntaxKind.EndOfFileToken, position);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.BadToken"/>.</returns>
    public static SyntaxToken Bad(int position, string text) => new(SyntaxKind.BadToken, position, text);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.WhiteSpaceToken"/>.</returns>
    public static SyntaxToken WhiteSpace(int position, string text) => new(SyntaxKind.WhiteSpaceToken, position, text);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.
    /// </summary>
    /// <param name="value">The integer value of this number token.</param>
    /// <param name="position">The position of this token in the input code.</param>
    /// <param name="text">The original text of this token from the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.NumberToken"/>.</returns>
    public static SyntaxToken Number(int value, int position, string text) => new(SyntaxKind.NumberToken, position, text, value);
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.PlusToken"/>.</returns>
    public static SyntaxToken Plus(int position) => new(SyntaxKind.PlusToken, position, "+");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.MinusToken"/>.</returns>
    public static SyntaxToken Minus(int position) => new(SyntaxKind.MinusToken, position, "-");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.StarToken"/>.</returns>
    public static SyntaxToken Star(int position) => new(SyntaxKind.StarToken, position, "*");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.SlashToken"/>.</returns>
    public static SyntaxToken Slash(int position) => new(SyntaxKind.SlashToken, position, "/");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.OpenParenthesisToken"/>.</returns>
    public static SyntaxToken OpenParenthesis(int position) => new(SyntaxKind.OpenParenthesisToken, position, "(");
    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.
    /// </summary>
    /// <param name="position">The position of this token in the input code.</param>
    /// <returns>A <see cref="SyntaxToken"/> of <see cref="SyntaxKind"/> <see cref="SyntaxKind.ClosedParenthesisToken"/>.</returns>
    public static SyntaxToken ClosedParenthesis(int position) => new(SyntaxKind.ClosedParenthesisToken, position, ")");

    /// <inheritdoc/>
    public override SyntaxKind Kind { get; }
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

    /// <summary>
    /// The original text in the input code.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// The position of this token in the input stream.
    /// </summary>
    public int Position { get; }
    /// <summary>
    /// The value of this token, if there is any.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> with the given values.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of this token.</param>
    /// <param name="position">The position of this token in the input stream.</param>
    /// <param name="text">The original text in the input code.</param>
    public SyntaxToken(SyntaxKind kind, int position = 0, string text = "", object? value = null) => (Kind, Text, Position, Value) = (kind, text, position, value);

    /// <inheritdoc />
    public override string ToString() => $"{Position}: {Kind} \"{Text}\" ({Value})";
}
