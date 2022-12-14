namespace Balu.Text;

/// <summary>
/// Represents a span of text in a Balu code string.
/// </summary>
public readonly record struct TextSpan
{
    /// <summary>
    /// The start position in the Balu string.
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// The length of this span in the Balu string.
    /// </summary>
    public int Length { get; }
    /// <summary>
    /// The first position after this span in the Balu string.
    /// </summary>
    public int End => Start + Length;

    /// <summary>
    /// Initializes a new <see cref="TextSpan"/> with the given <paramref name="start"/> and <paramref name="length"/> values.
    /// </summary>
    /// <param name="start">The starting position of the span.</param>
    /// <param name="length">The length of the span.</param>
    public TextSpan(int start, int length) => (Start, Length) = (start, length);

    /// <inheritdoc />
    public override string ToString() => $"({Start}, {Length})";
}
