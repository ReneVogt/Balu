using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Balu.Interactive;
using Xunit;

namespace bi.Tests;

public sealed class SubmissionViewTests
{
    [Fact]
    public void LongLineIsClippedAndCursorRemainsVisible()
    {
        var console = new TestConsole(width: 8, height: 4);
        var view = CreateView(console, "0123456789");

        view.CursorX = view.SubmissionDocument[0].Length;

        Assert.Equal(0, console.InvalidPositionAttempts);
        Assert.Equal(0, console.WrapCount);
        Assert.InRange(console.CursorLeft, 0, console.BufferWidth - 1);
        Assert.Equal("» 6789 ", console.GetRow(0)[..7]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void NarrowWindowDoesNotProduceInvalidCoordinates(int width)
    {
        var console = new TestConsole(width, height: 2);
        var view = CreateView(console, "a long line");

        view.CursorX = view.SubmissionDocument[0].Length;

        Assert.Equal(0, console.InvalidPositionAttempts);
        Assert.Equal(0, console.WrapCount);
        Assert.InRange(console.CursorLeft, 0, width - 1);
    }

    [Fact]
    public void TallDocumentUsesVerticalViewport()
    {
        var console = new TestConsole(width: 12, height: 3, cursorTop: 2);
        var view = CreateView(console, "line0", "line1", "line2", "line3", "line4", "line5");

        view.CursorY = 5;

        Assert.Equal(0, console.InvalidPositionAttempts);
        Assert.Equal(0, console.WrapCount);
        Assert.Equal(2, console.CursorTop);
        Assert.StartsWith("· line3", console.GetRow(0), StringComparison.Ordinal);

        view.CursorY = 1;

        Assert.Equal(0, console.CursorTop);
        Assert.StartsWith("· line1", console.GetRow(0), StringComparison.Ordinal);
    }

    [Fact]
    public void RemovingLinesClearsPreviouslyRenderedRows()
    {
        var console = new TestConsole(width: 12, height: 3);
        var view = CreateView(console, "line0", "line1", "line2");

        using (view.CreateUpdateContext())
        {
            view.CursorY = 0;
            view.SubmissionDocument.RemoveAt(2);
            view.SubmissionDocument.RemoveAt(1);
        }

        Assert.Equal(new string(' ', 11), console.GetRow(1)[..11]);
        Assert.Equal(new string(' ', 11), console.GetRow(2)[..11]);
    }

    [Fact]
    public void ResizeKeepsLongLineInsideNewWindowBounds()
    {
        var console = new TestConsole(width: 12, height: 3);
        var view = CreateView(console, "01234567890123456789");
        view.CursorX = view.SubmissionDocument[0].Length;

        console.WindowWidth = 3;
        view.SubmissionDocument[0] += "x";

        Assert.Equal(0, console.InvalidPositionAttempts);
        Assert.Equal(0, console.WrapCount);
        Assert.InRange(console.CursorLeft, 0, console.WindowWidth - 1);
    }

    [Fact]
    public void HeightResizeDoesNotForgetRowsThatStillNeedClearing()
    {
        var console = new TestConsole(width: 12, height: 3);
        var view = CreateView(console, "line0", "line1", "line2");

        console.WindowHeight = 1;
        using (view.CreateUpdateContext())
        {
            view.CursorY = 0;
            view.SubmissionDocument.RemoveAt(2);
            view.SubmissionDocument.RemoveAt(1);
        }
        console.WindowHeight = 3;
        view.SubmissionDocument[0] = "updated";

        Assert.Equal(new string(' ', 11), console.GetRow(1)[..11]);
        Assert.Equal(new string(' ', 11), console.GetRow(2)[..11]);
    }

    [Fact]
    public void CursorFailureDuringResizeDoesNotLoseSubmission()
    {
        var console = new TestConsole(width: 12, height: 3);
        var view = CreateView(console, "text");
        console.CursorMoveFailuresRemaining = 2;

        view.SubmissionDocument[0] = "edited";

        Assert.Equal("edited", view.SubmissionDocument[0]);
        Assert.True(console.CursorVisible);
    }

    [Fact]
    public void RenderingFailureRestoresConsoleState()
    {
        var console = new TestConsole(width: 10, height: 3)
        {
            CursorVisible = true,
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkBlue
        };
        var document = new ObservableCollection<string>(["text"]);

        _ = Assert.Throws<InvalidOperationException>(() => new SubmissionView(document, ThrowingRenderer, console));

        Assert.True(console.CursorVisible);
        Assert.Equal(ConsoleColor.Yellow, console.ForegroundColor);
        Assert.Equal(ConsoleColor.DarkBlue, console.BackgroundColor);
    }

    static SubmissionView CreateView(TestConsole console, params string[] lines) =>
        new(new ObservableCollection<string>(lines), RenderLine, console);

    static object? RenderLine(IReadOnlyList<string> lines, int lineIndex, int start, int length, TextWriter writer, object? state)
    {
        writer.Write(lines[lineIndex].AsSpan(start, length));
        return state;
    }

    static object? ThrowingRenderer(IReadOnlyList<string> lines, int lineIndex, int start, int length, TextWriter writer, object? state) =>
        throw new InvalidOperationException("Rendering failed.");

    sealed class TestConsole : IConsole
    {
        sealed class ConsoleWriter(TestConsole console) : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
            public override void Write(char value) => console.Write(value);
            public override void Write(string? value)
            {
                if (value is null) return;
                foreach (var character in value) console.Write(character);
            }
            public override void Write(ReadOnlySpan<char> buffer)
            {
                foreach (var character in buffer) console.Write(character);
            }
        }

        readonly char[,] buffer;
        int cursorLeft;
        int cursorTop;

        public TestConsole(int width, int height, int cursorTop = 0)
        {
            BufferWidth = width;
            BufferHeight = height;
            WindowWidth = width;
            WindowHeight = height;
            buffer = new char[height, width];
            for (int row = 0; row < height; row++) ClearRow(row);
            this.cursorTop = cursorTop;
            Out = new ConsoleWriter(this);
        }

        public int BufferWidth { get; }
        public int BufferHeight { get; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public int CursorLeft => cursorLeft;
        public int CursorTop => cursorTop;
        public bool CursorVisible { get; set; } = true;
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public TextWriter Out { get; }
        public int InvalidPositionAttempts { get; private set; }
        public int WrapCount { get; private set; }
        public int CursorMoveFailuresRemaining { get; set; }

        public bool TrySetCursorPosition(int left, int top)
        {
            if (CursorMoveFailuresRemaining > 0)
            {
                CursorMoveFailuresRemaining--;
                return false;
            }
            if (!ValidatePosition(left, top)) return false;
            cursorLeft = left;
            cursorTop = top;
            return true;
        }

        public void WriteLine()
        {
            cursorLeft = 0;
            if (cursorTop < BufferHeight - 1)
            {
                cursorTop++;
                return;
            }

            for (int row = 1; row < BufferHeight; row++)
                for (int column = 0; column < BufferWidth; column++)
                    buffer[row - 1, column] = buffer[row, column];
            ClearRow(BufferHeight - 1);
        }

        public string GetRow(int row)
        {
            var result = new char[BufferWidth];
            for (int column = 0; column < BufferWidth; column++) result[column] = buffer[row, column];
            return new(result);
        }

        void Write(char value)
        {
            buffer[cursorTop, cursorLeft] = value;
            if (cursorLeft < BufferWidth - 1)
            {
                cursorLeft++;
                return;
            }

            WrapCount++;
            cursorLeft = 0;
            if (cursorTop < BufferHeight - 1) cursorTop++;
        }

        bool ValidatePosition(int left, int top)
        {
            if (left >= 0 && left < BufferWidth && top >= 0 && top < BufferHeight) return true;
            InvalidPositionAttempts++;
            return false;
        }

        void ClearRow(int row)
        {
            for (int column = 0; column < BufferWidth; column++) buffer[row, column] = ' ';
        }
    }
}
