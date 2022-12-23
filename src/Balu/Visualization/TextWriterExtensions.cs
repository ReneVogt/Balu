using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balu.Syntax;

namespace Balu.Visualization;

public static class TextWriterExtensions
{
    public static bool IsConsole(this TextWriter textWriter) => textWriter == Console.Out || textWriter is IndentedTextWriter {InnerWriter: var writer} && (writer == Console.Out || writer == Console.Error);
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
    public static void WriteDiagnostics(this TextWriter textWriter, IEnumerable<Diagnostic> diagnostics, SyntaxTree syntaxTree)
    {
        _ = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        _ = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _ = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));

        const string indent = "    ";

        foreach (var diagnostic in diagnostics.OrderBy(diagnostic => diagnostic.Location.Text.FileName).ThenBy(diagnostic => diagnostic.Location.Span.Start).ThenBy(diagnostic => diagnostic.Location.Span.Length))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            int lineNumber = syntaxTree.Text.GetLineIndex(diagnostic.Location.Span.Start);
            var syntaxLine = syntaxTree.Text.Lines[lineNumber];
            int column = diagnostic.Location.Span.Start - syntaxLine.Start;
            Console.WriteLine($"[{diagnostic.Id}] {diagnostic.Location}: {diagnostic.Message}");
            Console.ResetColor();
            if (diagnostic.Location.Span.Length > 0)
            {
                Console.Write(indent);
                Console.Write(syntaxTree.Text.ToString(syntaxLine.Start, column));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(syntaxTree.Text.ToString(diagnostic.Location.Span));
                Console.ResetColor();
                Console.WriteLine(syntaxTree.Text.ToString(diagnostic.Location.Span.End, Math.Max(0, syntaxLine.End - diagnostic.Location.Span.End)));
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