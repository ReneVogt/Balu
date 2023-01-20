using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;
using Xunit;

namespace Balu.Tests.UnitTests.Text;

public class SourceTextTests
{
    [Theory]
    [MemberData(nameof(ProvideParseTests))]
    public void SourceText_Parser_CorrectLines(string text, (int start, int length, int incl)[] lines)
    {
        var sourceText = SourceText.From(text);
        Assert.Equal(lines.Length, sourceText.Lines.Length);
        foreach (var zip in lines.Zip(sourceText.Lines, (expected, actual) => (expected, actual)))
        {
            Assert.Equal(zip.expected.start, zip.actual.Start);
            Assert.Equal(zip.expected.length, zip.actual.Length);
            Assert.Equal(zip.expected.incl, zip.actual.LengthIncludingNewLine);
        }
    }

    [Theory]
    [MemberData(nameof(ProvideLineIndexTests))]
    public void SourceText_GetLineIndex_CorrectIndices(string text, int position, int expectedIndex)
    {
        var sourceText = SourceText.From(text);
        Assert.Equal(expectedIndex, sourceText.GetLineIndex(position));
    }
    [Fact]
    public void SourceText_GetLineIndex_ArgumentOutOfRangeExceptions()
    {
        const string text = "some\r\nlines\r\n";
        var sourceText = SourceText.From(text);
        Assert.Throws<ArgumentOutOfRangeException>("position", () => sourceText.GetLineIndex(text.Length + 1));
    }

    public static IEnumerable<object[]> ProvideParseTests() =>
        new (string text, (int start, int length, int incl)[] lines)[]
        {
            ("", new (int start, int length, int incl)[] { (0, 0, 0) }),
            ("\n", new (int start, int length, int incl)[] { (0, 0, 1), (1, 0, 0) }),
            ("\r\n", new (int start, int length, int incl)[] { (0, 0, 2), (2, 0, 0) }),
            ("\n\n", new (int start, int length, int incl)[] { (0, 0, 1), (1, 0, 1), (2, 0, 0) }),
            ("\n\r\n", new (int start, int length, int incl)[] { (0, 0, 1), (1, 0, 2), (3, 0, 0) }),
            ("\r\n\n", new (int start, int length, int incl)[] { (0, 0, 2), (2, 0, 1), (3, 0, 0) }),
            ("\r\n\r\n", new (int start, int length, int incl)[] { (0, 0, 2), (2, 0, 2), (4, 0, 0) }),
            ("line1a\r1b\nline2\r\nline3_\n\nline5__\r\n\r\nline7___", new (int start, int length, int incl)[]
                {
                    (0, 9, 10),
                    (10, 5, 7),
                    (17, 6, 7),
                    (24, 0, 1),
                    (25, 7, 9),
                    (34, 0, 2),
                    (36, 8, 8)
                }),
            ("ending\nwith\r\n", new (int start, int length, int incl)[]
                {
                    (0, 6, 7),
                    (7, 4, 6),
                    (13, 0, 0)
                }),
            ("ending\nwith\n", new (int start, int length, int incl)[]
                {
                    (0, 6, 7),
                    (7, 4, 5),
                    (12, 0, 0)
                })
        }.Select(x => new object[] { x.text, x.lines });
    public static IEnumerable<object[]> ProvideLineIndexTests() =>
        new (string text, int[] expectedIndices)[]
        {
            ("", new[] { 0 }),
            ("\n", new[] { 0, 1 }),
            ("\r\n", new[] { 0, 0, 1 }),

            ("\n\n", new[] { 0, 1, 2 }),
            ("\r\n\n", new[] { 0, 0, 1, 2 }),
            ("\n\r\n", new[] { 0, 1, 1, 2 }),
            ("\r\n\r\n", new[] { 0, 0, 1, 1, 2 }),

            ("a\na\n", new[] { 0, 0, 1, 1, 2 }),
            ("a\na\r\n", new[] { 0, 0, 1, 1, 1, 2 }),
            ("a\r\na\n", new[] { 0, 0, 0, 1, 1, 2 }),
            ("a\r\na\r\n", new[] { 0, 0, 0, 1, 1, 1, 2 }),

            ("aa\naa\n", new[] { 0, 0, 0, 1, 1, 1, 2 }),
            ("aa\naa\r\n", new[] { 0, 0, 0, 1, 1, 1, 1, 2 }),
            ("aa\r\naa\n", new[] { 0, 0, 0, 0, 1, 1, 1, 2 }),
            ("aa\r\naa\r\n", new[] { 0, 0, 0, 0, 1, 1, 1, 1, 2 }),

            ("aa\naa\na", new[] { 0, 0, 0, 1, 1, 1, 2, 2 }),
            ("aa\naa\r\na", new[] { 0, 0, 0, 1, 1, 1, 1, 2, 2 }),
            ("aa\r\naa\na", new[] { 0, 0, 0, 0, 1, 1, 1, 2, 2 }),
            ("aa\r\naa\r\na", new[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2 }),

            ("aa\naa\na\n", new[] { 0, 0, 0, 1, 1, 1, 2, 2, 3 }),
            ("aa\naa\r\na\r\n", new[] { 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3 }),

            ("a\naa\naa\na", new[] { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3 }),
            ("a\naa\naa\r\na", new[] { 0, 0, 1, 1, 1, 2, 2, 2, 2, 3, 3}),
            ("a\naa\r\naa\na", new[] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3 }),
            ("a\naa\r\naa\r\na", new[] { 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3 })

        }.SelectMany(x => x.expectedIndices.Select((expectedIndex, i) => new object[] { x.text, i, expectedIndex }));
}
