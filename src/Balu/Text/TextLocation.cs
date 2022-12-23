namespace Balu.Text;

public readonly record struct TextLocation(SourceText Text, TextSpan Span)
{
    public int StartLine => Text.GetLineIndex(Span.Start);
    public int EndLine => Text.GetLineIndex(Span.End);
    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
    public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

    public override string ToString() => $"{Text.FileName}{Span}";
}
