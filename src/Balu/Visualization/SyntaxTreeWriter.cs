using System;
using System.IO;
using System.Linq;
using Balu.Syntax;

namespace Balu.Visualization;

/// <summary>
/// Provides text-based tree representation of an
/// <see cref="ExpressionSyntax"/>.
/// </summary>
public sealed class SyntaxTreeWriter : SyntaxVisitor
{
    readonly TextWriter writer;
    readonly bool console;
    string indent = string.Empty;
    bool last = true;

    SyntaxTreeWriter(TextWriter writer) => (this.writer, console) = (writer, writer == Console.Out);

    /// <inheritdoc />
    public override SyntaxNode Visit(SyntaxNode node)
    {
        var marker = last ? TreeTexts.LastLeaf : TreeTexts.Leaf;
        writer.Write(indent);
        if (console) Console.ForegroundColor = ConsoleColor.DarkGray;
        writer.Write(marker);
        if (console) Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
        writer.Write($"{node.Kind}{node.Span}");
        if (node is SyntaxToken { Text: var text, Value: var value})
            writer.Write($" \"{text}\" {value}");
        if (console) Console.ResetColor();
        writer.WriteLine();

        var lastIndnet = indent;
        var lastLast = last;
        indent += last ? TreeTexts.Indent : TreeTexts.Branch;
        last = false;

        var children = node.Children.ToArray();
        for (int i = 0; i < children.Length - 1; i++)
            Visit(children[i]);
        if (children.Length > 0)
        {
            last = true;
            Visit(children[^1]);
        }

        last = lastLast;
        indent = lastIndnet;
        return node;
    }

    /// <summary>
    /// Writes a text-based tree representation of <paramref name="syntax"/> to <paramref name="textWriter"/>.
    /// </summary>
    /// <param name="syntax">The <see cref="SyntaxNode"/> to represent.</param>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write the output to.</param>
    public static void Print(SyntaxNode syntax, TextWriter textWriter) => new SyntaxTreeWriter(textWriter).Visit(syntax);
}
