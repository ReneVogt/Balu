namespace Balu.Text;

public readonly record struct TextLocation(SourceText Text, TextSpan Span)
{
    public static TextLocation None => default;
    public bool IsNone => Text is null;

    public string FileName => Text.FileName;
    public int StartLine => Text.GetLineIndex(Span.Start);
    public int EndLine => Text.GetLineIndex(Span.End);
    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
    public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

    public override string ToString()
    {
        if (IsNone) return string.Empty;

        var line = Text.GetLineIndex(Span.Start);
        return $"{Text.FileName}({line + 1},{Span.Start - Text.Lines[line].Start + 1})";
    }

    public static TextLocation operator +(TextLocation left, TextLocation right) => Add(left, right);
    public static TextLocation Add(TextLocation left, TextLocation right) => left with { Span = left.Span + right.Span };
}
