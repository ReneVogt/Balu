namespace Balu;

/// <summary>
/// Represents a syntax token in Balu code.
/// </summary>
public sealed class SyntaxToken : SyntaxNode
{
    /// <inheritdoc/>
    public override SyntaxKind Kind { get; }
    /// <summary>
    /// The original text in the input code.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// The position of this token in the input stream.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Creates a new <see cref="SyntaxToken"/> with the given values.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> of this token.</param>
    /// <param name="position">The position of this token in the input stream.</param>
    /// <param name="text">The original text in the input code.</param>
    internal SyntaxToken(SyntaxKind kind, int position = 0, string text = "") => (Kind, Text, Position) = (kind, text, position);

    /// <inheritdoc />
    public override string ToString() => $"{Kind} {Position} {Text}";
}
