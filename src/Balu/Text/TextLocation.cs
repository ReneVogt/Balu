namespace Balu.Text;

public readonly record struct TextLocation(SourceText? Text, TextSpan Span)
{
    public string? FileName => Text?.FileName;
    public int StartLine => Text?.GetLineIndex(Span.Start) ?? 0;
    public int EndLine => Text?.GetLineIndex(Span.End) ?? 0;
    public int StartCharacter => Span.Start - Text?.Lines[StartLine].Start ?? 0;
    public int EndCharacter => Span.End - Text?.Lines[EndLine].Start ?? 0;

    public override string ToString()
    {
        if (Text is null) return string.Empty; // happens for non-located diagnostics like "missing entry point"
        var line = Text.GetLineIndex(Span.Start);
        return $"{Text.FileName}({line + 1},{Span.Start - Text.Lines[line].Start + 1})";
    }

    public static TextLocation operator +(TextLocation left, TextLocation right) => Add(left, right);
    public static TextLocation Add(TextLocation left, TextLocation right) => left with { Span = left.Span + right.Span };
}
