using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Balu.Text;

public sealed class SourceText
{
    readonly string text;

    public string FileName { get; }
    public ImmutableArray<TextLine> Lines { get; }
    internal ImmutableArray<byte> Checksum { get; }
    public char this[int index] => text[index];
    public int Length => text.Length;

    SourceText(string text, string fileName, ImmutableArray<byte> checksum, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.text = text;
        FileName = fileName;
        Checksum = checksum;
        Lines = ParseLines(this, cancellationToken);
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

    internal int GetLineBreakWidth(int position)
    {
        if (position < 0 || position >= Length)
            return 0;

        if (text[position] == '\r')
            return position + 1 < Length && text[position + 1] == '\n' ? 2 : 1;

        return text[position] == '\n' ? 1 : 0;
    }

    static ImmutableArray<TextLine> ParseLines(SourceText sourceText, CancellationToken cancellationToken)
    {
        int lineStart = 0, position = 0;
        var builder = ImmutableArray.CreateBuilder<TextLine>();
        while (position < sourceText.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var endings = sourceText.GetLineBreakWidth(position);
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
            builder.Add(new(sourceText, lineStart, sourceText.Length - lineStart, sourceText.Length - lineStart));
        return builder.ToImmutable();
    }

    internal static SourceText Load(string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = File.OpenRead(fileName ?? throw new ArgumentNullException(nameof(fileName)));
        var checksum = ComputeChecksum(stream, cancellationToken);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        cancellationToken.ThrowIfCancellationRequested();
        return new(text, fileName, checksum, cancellationToken);
    }
    public static SourceText From(string text, string fileName = "", CancellationToken cancellationToken = default)
    {
        _ = text ?? throw new ArgumentNullException(nameof(text));
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        cancellationToken.ThrowIfCancellationRequested();
        return new(text, fileName, ComputeChecksum(Encoding.UTF8.GetBytes(text), cancellationToken), cancellationToken);
    }

    static ImmutableArray<byte> ComputeChecksum(Stream stream, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var algorithm = SHA256.Create();
        var checksum = algorithm.ComputeHash(stream);
        cancellationToken.ThrowIfCancellationRequested();
        return ImmutableArray.CreateRange(checksum);
    }
    static ImmutableArray<byte> ComputeChecksum(byte[] bytes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var algorithm = SHA256.Create();
        var checksum = algorithm.ComputeHash(bytes);
        cancellationToken.ThrowIfCancellationRequested();
        return ImmutableArray.CreateRange(checksum);
    }
}
