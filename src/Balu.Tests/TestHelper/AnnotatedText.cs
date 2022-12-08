using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Balu.Text;

namespace Balu.Tests.TestHelper;

sealed class AnnotatedText
{
    public string Text { get; }
    public ImmutableArray<TextSpan> Spans { get; }

    AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        Text = text;
        Spans = spans;
    }

    public static AnnotatedText Parse(string text)
    {
        var unindented = Unindent(text);
        int position = 0;
        Stack<int> startStack = new();
        List<TextSpan> spans = new();
        StringBuilder resultBuilder = new();

        foreach (char c in unindented)
            switch (c)
            {
                case '[':
                    startStack.Push(position);
                    break;
                case ']':
                    if (startStack.Count <= 0)
                        throw new ArgumentException("Too many ']' in annotated text.", nameof(text));
                    var start = startStack.Pop();
                    spans.Add(new(start, position-start));
                    break;
                default:
                    resultBuilder.Append(c);
                    position++;
                    break;
            }

        if (startStack.Count != 0)
            throw new ArgumentException("Missing ']' in annotated text.", nameof(text));

        return new(resultBuilder.ToString(), spans.ToImmutableArray());
    }
    public static string[] UnindentLines(string? text)
    {
        if (text is null) return Array.Empty<string>();
        using var reader = new StringReader(text);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
            lines.Add(line);
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) lines.RemoveAt(0);
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);

        if (lines.Count == 0) return Array.Empty<string>();
        var indentation = lines.Min(l => l.Length - l.TrimStart().Length);
        return lines.Select(l => l[indentation..]).ToArray();
    }
    static string Unindent(string text) => string.Join(Environment.NewLine, UnindentLines(text));
}
