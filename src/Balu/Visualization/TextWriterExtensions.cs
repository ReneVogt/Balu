using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Balu.Visualization;

static class TextWriterExtensions
{
    public static bool IsConsoleOut(this TextWriter textWriter) => textWriter == Console.Out || textWriter is IndentedTextWriter {InnerWriter: var writer} && writer == Console.Out;
    public static void SetForegroundColor(this TextWriter textWriter, ConsoleColor foregroundColor)
    {
        if (!textWriter.IsConsoleOut()) return;
        Console.ForegroundColor = foregroundColor;
    }
    public static void ResetColor(this TextWriter textWriter)
    {
        if (!textWriter.IsConsoleOut()) return;
        Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter textWriter, string text) => textWriter.WriteColoredText(text, ConsoleColor.Blue);
    public static void WriteIdentifier(this TextWriter textWriter, string text) => textWriter.WriteColoredText(text, ConsoleColor.DarkYellow);
    public static void WriteNumber(this TextWriter textWriter, string text) => textWriter.WriteColoredText(text, ConsoleColor.Cyan);
    public static void WriteString(this TextWriter textWriter, string text) => textWriter.WriteColoredText(text, ConsoleColor.Magenta);
    public static void WritePunctuation(this TextWriter textWriter, string text) => textWriter.WriteColoredText(text, ConsoleColor.DarkGray);
    static void WriteColoredText(this TextWriter textWriter, string text, ConsoleColor foregroundColor)
    {
        textWriter.SetForegroundColor(foregroundColor);
        textWriter.Write(text);
        textWriter.ResetColor();
    }
}