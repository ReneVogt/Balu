using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Balu.Visualization;

public static class TextWriterExtensions
{
    public static bool IsConsole(this TextWriter textWriter)
    {
        var writer = textWriter is IndentedTextWriter iw ? iw.InnerWriter : textWriter;
        if (writer == Console.Out) return !Console.IsOutputRedirected;
        if (writer == Console.Error) return !(Console.IsErrorRedirected || Console.IsOutputRedirected);
        return false;
    }
    public static void SetForegroundColor(this TextWriter textWriter, ConsoleColor foregroundColor)
    {
        if (!textWriter.IsConsole()) return;
        Console.ForegroundColor = foregroundColor;
    }
    public static void ResetColor(this TextWriter textWriter)
    {
        if (!textWriter.IsConsole()) return;
        Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter textWriter, string? text) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).WriteColoredText(text, ConsoleColor.Blue);
    public static void WriteIdentifier(this TextWriter textWriter, string? text) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).WriteColoredText(text, ConsoleColor.DarkYellow);
    public static void WriteNumber(this TextWriter textWriter, string? text) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).WriteColoredText(text, ConsoleColor.Cyan);
    public static void WriteString(this TextWriter textWriter, string? text) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).WriteColoredText(text, ConsoleColor.Magenta);
    public static void WritePunctuation(this TextWriter textWriter, string? text) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).WriteColoredText(text, ConsoleColor.DarkGray);
    public static void WriteSpace(this TextWriter textWriter) => (textWriter ?? throw new ArgumentNullException(nameof(textWriter))).Write(' ');

#pragma warning disable CA1303 // writing literals to console
    public static void WriteDiagnostics(this TextWriter textWriter, IEnumerable<Diagnostic> diagnostics)
    {
        _ = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        _ = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        const string indent = "    ";

        foreach (var diagnostic in diagnostics.OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenBy(diagnostic => diagnostic.Location.Span.Length))
        {
            var sourceText = diagnostic.Location.Text;
            Console.ForegroundColor = ConsoleColor.Red;
            int lineNumber = sourceText.GetLineIndex(diagnostic.Location.Span.Start);
            var syntaxLine = sourceText.Lines[lineNumber];
            int column = diagnostic.Location.Span.Start - syntaxLine.Start;
            Console.WriteLine($"[{diagnostic.Id}] {diagnostic.Location}: {diagnostic.Message}");
            Console.ResetColor();
            if (diagnostic.Location.Span.Length > 0)
            {
                Console.Write(indent);
                Console.Write(sourceText.ToString(syntaxLine.Start, column));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(sourceText.ToString(diagnostic.Location.Span));
                Console.ResetColor();
                Console.WriteLine(sourceText.ToString(diagnostic.Location.Span.End, Math.Max(0, syntaxLine.End - diagnostic.Location.Span.End)));
                Console.ResetColor();
            }
        }
    }

    static void WriteColoredText(this TextWriter textWriter, string? text, ConsoleColor foregroundColor)
    {
        textWriter.SetForegroundColor(foregroundColor);
        textWriter.Write(text ?? "?");
        textWriter.ResetColor();
    }
}