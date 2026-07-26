using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Balu.Text;

public sealed class SourceText
{
    readonly string text;

    public string FileName { get; }
    public ImmutableArray<TextLine> Lines { get; }
    internal ImmutableArray<byte> Checksum { get; }
    public char this[int index] => text[index];
    public int Length => text.Length;

    SourceText(string text, string fileName, ImmutableArray<byte> checksum)
    {
        this.text = text;
        FileName = fileName;
        Checksum = checksum;
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
        if (builder.Count == 0 || builder.Last().LengthIncludingNewLine > builder.Last().Length)
            builder.Add(new(sourceText, lineStart, text.Length - lineStart, text.Length - lineStart));
        return builder.ToImmutable();
    }

    internal static SourceText Load(string fileName)
    {
        using var stream = File.OpenRead(fileName ?? throw new ArgumentNullException(nameof(fileName)));
        var checksum = ComputeChecksum(stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return new(reader.ReadToEnd(), fileName, checksum);
    }
    public static SourceText From(string text, string fileName = "")
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        return new(text, fileName, ComputeChecksum(Encoding.UTF8.GetBytes(text)));
    }

    static ImmutableArray<byte> ComputeChecksum(Stream stream)
    {
        using var algorithm = SHA256.Create();
        return ImmutableArray.CreateRange(algorithm.ComputeHash(stream));
    }
    static ImmutableArray<byte> ComputeChecksum(byte[] bytes)
    {
        using var algorithm = SHA256.Create();
        return ImmutableArray.CreateRange(algorithm.ComputeHash(bytes));
    }
}
