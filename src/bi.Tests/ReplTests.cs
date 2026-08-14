using System;
using System.IO;
using Balu.Interactive;
using Xunit;

namespace bi.Tests;

public sealed class ReplTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void HelpMakesProgressInNarrowWindows(int windowWidth)
    {
        var console = new TestConsole(windowWidth);

        new TestRepl().EvaluateHelp(console);

        Assert.Contains("#help", console.Output, StringComparison.Ordinal);
        Assert.Contains("  S", console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void HelpFallsBackWhenWindowWidthCannotBeRead()
    {
        var console = new TestConsole(windowWidth: null);

        new TestRepl().EvaluateHelp(console);

        Assert.Contains("#help", console.Output, StringComparison.Ordinal);
        Assert.Contains("  Shows this help.", console.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void HelpWrapsDescriptionsAtOrdinaryWidths()
    {
        var console = new TestConsole(windowWidth: 10);

        new TestRepl().EvaluateHelp(console);

        Assert.Contains($"  Clears {Environment.NewLine}  the inp", console.Output, StringComparison.Ordinal);
    }

    sealed class TestRepl : Repl
    {
        protected override bool IsCompleteSubmission(string text) => true;
        protected override void EvaluateSubmission(string text) { }
    }

    sealed class TestConsole(int? windowWidth) : IConsole
    {
        readonly StringWriter output = new();

        public string Output => output.ToString();
        public int BufferWidth => windowWidth ?? 80;
        public int BufferHeight => 25;
        public int WindowWidth => windowWidth ?? throw new IOException();
        public int WindowHeight => 25;
        public int CursorLeft => 0;
        public int CursorTop => 0;
        public bool CursorVisible { get; set; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public TextWriter Out => output;

        public bool TryGetWindowWidth(out int width)
        {
            width = windowWidth.GetValueOrDefault();
            return windowWidth.HasValue;
        }

        public bool TrySetCursorPosition(int left, int top) => true;
        public void WriteLine() => output.WriteLine();
    }
}
