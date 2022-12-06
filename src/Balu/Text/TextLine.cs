namespace Balu.Text;

/// <summary>
/// Represents a Balu source code line.
/// </summary>
public sealed class TextLine
{
    /// <summary>
    /// The <see cref="SourceText"/> this line belongs to.
    /// </summary>
    public SourceText Text { get; }
    /// <summary>
    /// The start position of this line in the <see cref="Text"/>.
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// The length of this line (without any new line characters).
    /// </summary>
    public int Length { get; }
    /// <summary>
    /// The length of this line (including all new line characters).
    /// </summary>
    public int LengthIncludingNewLine { get; }
    /// <summary>
    /// The end position of this line (without any new line characters).
    /// </summary>
    public int End { get; }
    /// <summary>
    /// The end position of this line (including all new line characters).
    /// </summary>
    public int EndIncludingNewLine { get; }
    /// <summary>
    /// The <see cref="TextSpan"/> this line covers (without any new line characters).
    /// </summary>
    public TextSpan Span { get; }

    internal TextLine(SourceText text, int start, int length, int lengthIncludingNewLine)
    {
        Text = text;
        Start = start;
        Length = length;
        LengthIncludingNewLine = lengthIncludingNewLine;

        End = Start + Length;
        EndIncludingNewLine = Start + LengthIncludingNewLine;
        Span = new (start, Length);
    }

}
