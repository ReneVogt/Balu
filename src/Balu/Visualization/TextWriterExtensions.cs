using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Balu.Visualization;

public static class TextWriterExtensions
{
    // ReSharper disable once TailRecursiveCall
    public static bool IsConsole(this TextWriter textWriter) => textWriter is IndentedTextWriter iw
                                                                    ? IsConsole(iw.InnerWriter)
                                                                    : textWriter == Console.Out
                                                                        ? !Console.IsOutputRedirected
                                                                        : textWriter == Console.Error &&
                                                                          !(Console.IsOutputRedirected || Console.IsErrorRedirected);
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

    public static void WriteDiagnostics(this TextWriter textWriter, IEnumerable<Diagnostic> diagnostics)
    {
        _ = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        _ = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        foreach (var diagnostic in diagnostics.OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length))
        {
            var sourceText = diagnostic.Location.Text;
            var startLine = sourceText.Lines[diagnostic.Location.StartLine];
            var endLine = sourceText.Lines[diagnostic.Location.EndLine];
            int column = diagnostic.Location.Span.Start - startLine.Start;
            WriteColoredText(textWriter, $"[{diagnostic.Id}] {diagnostic.Location}: {diagnostic.Message}", ConsoleColor.Red);
            textWriter.WriteLine();
            if (diagnostic.Location.Span.Length > 0)
            {
                textWriter.Write("    ");
                textWriter.Write(sourceText.ToString(startLine.Start, column));
                WriteColoredText(textWriter, sourceText.ToString(diagnostic.Location.Span), ConsoleColor.Red);
                textWriter.WriteLine(sourceText.ToString(diagnostic.Location.Span.End, Math.Max(0, endLine.End - diagnostic.Location.Span.End)));
            }
        }
    }

    public static void WriteColoredText(this TextWriter textWriter, string? text, ConsoleColor foregroundColor)
    {
        _ = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        textWriter.SetForegroundColor(foregroundColor);
        textWriter.Write(text ?? "?");
        textWriter.ResetColor();
    }
}