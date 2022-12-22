using System;
using System.Collections.Immutable;

namespace Balu.Text;

public sealed class SourceText
{
    readonly string text;

    public string FileName { get; }
    public ImmutableArray<TextLine> Lines { get; }
    public char this[int index] => text[index];
    public int Length => text.Length;

    SourceText(string text, string fileName)
    {
        this.text = text;
        FileName = fileName;
        Lines = ParseLines(this, text);
    }

    public int GetLineIndex(int position)
    {
        if (position < 0 || position > text.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        int lower = 0, upper = Lines.Length - 1;
        while (lower < upper)
        {
            var index = lower + (upper - lower) / 2;
            if (position < Lines[index].Start)
                upper = index - 1;
            else if (position >= Lines[index].EndIncludingNewLine)
                lower = index + 1;
            else return index;
        }

        return lower;
    }

    public override string ToString() => text;
    public string ToString(int start, int length) => text.Substring(start, length);
    public string ToString(TextSpan span) => ToString(span.Start, span.Length);

    static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        int lineStart = 0, position = 0;
        var builder = ImmutableArray.CreateBuilder<TextLine>();
        while(position < text.Length)
        {
            int endings = text[position] == '\n' ? 1 : text[position] == '\r' && position + 1 < text.Length && text[position+1] == '\n' ? 2 : 0;
            if (endings == 0)
            {
                position++;
                continue;
            }

            builder.Add(new(sourceText, lineStart, position - lineStart, position - lineStart + endings));
            position += endings;
            lineStart = position;
        }
        if (builder.Count == 0 || builder[^1].LengthIncludingNewLine > builder[^1].Length)
            builder.Add(new(sourceText, lineStart, text.Length - lineStart, text.Length - lineStart));
        return builder.ToImmutable();
    }

    public static SourceText From(string text, string fileName = "") => new(text ?? throw new ArgumentNullException(nameof(text)),
                                                                            fileName ?? throw new ArgumentNullException(nameof(fileName)));
}
