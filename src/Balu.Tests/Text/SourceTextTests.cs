using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;
using Xunit;

namespace Balu.Tests.Text;

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
            Assert.Equal(zip.expected.incl, zip.actual.LengthWithNewLine);
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
        Assert.Throws<ArgumentOutOfRangeException>("position", () => sourceText.GetLineIndex(text.Length+1));
    }

    public static IEnumerable<object[]> ProvideParseTests() =>
        new (string text, (int start, int length, int incl)[] lines)[]
        {
            ("", Array.Empty<(int, int, int)>()),
            ("\n", new (int start, int length, int incl)[] { (0, 0, 1) }),
            ("\r\n", new (int start, int length, int incl)[] { (0, 0, 2) }),
            ("test\rtest\nabc\r\nhuhu", new (int start, int length, int incl)[]
                {
                    (0, 9, 10),
                    (10, 3, 5),
                    (15, 4, 4)
                }),
            ("test\rtest\nabc\r\nhuhu\r\n", new (int start, int length, int incl)[]
                {
                    (0, 9, 10),
                    (10, 3, 5),
                    (15, 4, 6)
                }),
        }.Select(x => new object[] { x.text, x.lines });
    public static IEnumerable<object[]> ProvideLineIndexTests() =>
        new (string text, int position, int expectedIndex)[]
        {
            ("", 0, 0),
            ("\r\n", 0, 0),
            ("test\nhoppla\r\nwhat", 7, 1),
            ("test\r\ntest2\r\n", 4, 0),
            ("test\r\nwhat", 0, 0),
            ("test\r\nwhat\r\n", 12, 2),
            ("test\r\nwhat", 10, 1)
        }.Select(x => new object[] { x.text, x.position, x.expectedIndex });
}
