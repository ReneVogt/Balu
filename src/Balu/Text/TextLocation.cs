namespace Balu.Text;

public readonly record struct TextLocation(SourceText Text, TextSpan Span)
{
    public string FileName => Text.FileName;
    public int StartLine => Text.GetLineIndex(Span.Start);
    public int EndLine => Text.GetLineIndex(Span.End);
    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
    public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

    public override string ToString()
    {
        if (Text is null) return string.Empty; // happens for non-located diagnostics like "missing entry point"
        var line = Text.GetLineIndex(Span.Start);
        return $"{Text.FileName}({line + 1},{Span.Start - Text.Lines[line].Start + 1})";
    }
}
