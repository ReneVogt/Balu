using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balu.Diagnostics;

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
        var diags = (diagnostics ?? throw new ArgumentNullException(nameof(diagnostics))).ToArray();

        

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        foreach (var diagnostic in diags.Where(diagnostic => diagnostic.Location.Text != null).OrderBy(diagnostic => diagnostic.Location.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenByDescending(diagnostic => diagnostic.Location.Span.Length))
        {
            var sourceText = diagnostic.Location.Text;
            var startLine = sourceText.Lines[diagnostic.Location.StartLine];
            var endLine = sourceText.Lines[diagnostic.Location.EndLine];
            int column = diagnostic.Location.Span.Start - startLine.Start;
            bool warning = diagnostic.Severity == DiagnosticSeverity.Error;
            var color = warning ? ConsoleColor.Yellow : ConsoleColor.Red;
            WriteColoredText(textWriter, $"{diagnostic.Location}: {(warning ? "error" : "warning")} {diagnostic.IdString}: {diagnostic.Message}", color);
            textWriter.WriteLine();
            if (diagnostic.Location.Span.Length > 0)
            {
                textWriter.Write("    ");
                textWriter.Write(sourceText.ToString(startLine.Start, column));
                WriteColoredText(textWriter, sourceText.ToString(diagnostic.Location.Span), color);
                textWriter.WriteLine(sourceText.ToString(diagnostic.Location.Span.End, Math.Max(0, endLine.End - diagnostic.Location.Span.End)));
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        foreach (var diagnostic in diags.Where(diagnostic => diagnostic.Location.Text is null))
        {
            WriteColoredText(textWriter, $"[{diagnostic.IdString}]: {diagnostic.Message}",
                             diagnostic.Severity == DiagnosticSeverity.Error ? ConsoleColor.Red : ConsoleColor.Yellow);
            textWriter.WriteLine();
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