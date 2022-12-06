using System;
using System.Collections.Immutable;

namespace Balu.Text;

/// <summary>
/// Represents a Balu source code text.
/// </summary>
public sealed class SourceText
{
    /// <summary>
    /// The lines in this source code text.
    /// </summary>
    public ImmutableArray<TextLine> Lines { get; }
    /// <summary>
    /// The original Balu source code text.
    /// </summary>
    public string Text { get; }

    SourceText(string text)
    {
        Text = text;
        Lines = ParseLines(this, text);
    }

    public int GetLineIndex(int position)
    {
        if (position < 0 || position > Text.Length)
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

    /// <inheritdoc />
    public override string ToString() => Text;
    /// <summary>
    /// Returns a part of the represented Balu code text.
    /// </summary>
    /// <param name="start">The start position of the part to return.</param>
    /// <param name="length">The length of the part to return.</param>
    /// <returns>The part of the original text defined by <paramref name="start"/> and <paramref name="length"/>.</returns>
    public string ToString(int start, int length) => Text.Substring(start, length);
    /// <summary>
    /// Returns a part of the represented Balu code text.
    /// </summary>
    /// <param name="span">The span to return of the original text.</param>
    /// <returns>The part of the original text defined by <paramref name="span"/>.</returns>
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

        if (lineStart < position)
            builder.Add(new(sourceText, lineStart, text.Length - lineStart, text.Length - lineStart));
        return builder.ToImmutable();
    }

    /// <summary>
    /// Creates a <see cref="SourceText"/> instance from the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The Balu source code.</param>
    /// <returns>A new <see cref="SourceText"/> instance representing the Balu source code <paramref name="text"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    public static SourceText From(string text) => new SourceText(text ?? throw new ArgumentNullException(nameof(text)));
}
