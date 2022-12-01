using System;
using System.IO;
using System.Linq;
using Balu.Syntax;

namespace Balu.Visualization;

/// <summary>
/// Provides text-based tree representation of an
/// <see cref="ExpressionSyntax"/>.
/// </summary>
public sealed class SyntaxTreePrinter : SyntaxVisitor
{
    readonly TextWriter writer;
    string indent = string.Empty;
    bool last = true;

    SyntaxTreePrinter(TextWriter writer) => this.writer = writer;

    /// <inheritdoc />
    public override SyntaxNode Visit(SyntaxNode node)
    {
        var marker = last ? TreeTexts.LastLeaf : TreeTexts.Leaf;
        writer.Write(indent);
        writer.Write(marker);
        writer.WriteLine(node);

        var lastIndnet = indent;
        var lastLast = last;
        indent += last ? "   " : "│  ";
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
    /// <param name="syntax">The <see cref="ExpressionSyntax"/> to represent.</param>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write the output to.</param>
    public static void Print(ExpressionSyntax syntax, TextWriter textWriter) => new SyntaxTreePrinter(textWriter).Visit(syntax);
}
