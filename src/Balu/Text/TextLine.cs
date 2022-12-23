namespace Balu.Text;

public sealed class TextLine
{
    public SourceText Text { get; }
    public int Start { get; }
    public int Length { get; }
    public int LengthIncludingNewLine { get; }
    public int End { get; }
    public int EndIncludingNewLine { get; }
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

    public override string ToString() => Text.ToString(Span);
}
