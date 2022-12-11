using System;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Syntax;

namespace Balu.Visualization;

sealed class BoundTreeWriter : BoundTreeVisitor
{
    readonly TextWriter writer;
    readonly bool console;
    string indent = string.Empty;
    bool last = true;

    BoundTreeWriter(TextWriter writer) => (this.writer, console) = (writer, writer == Console.Out);

    /// <inheritdoc />
    public override BoundNode Visit(BoundNode node)
    {
        var marker = last ? TreeTexts.LastLeaf : TreeTexts.Leaf;
        writer.Write(indent);
        if (console) Console.ForegroundColor = ConsoleColor.DarkGray;
        writer.Write(marker);
        if (console)
            Console.ForegroundColor = node switch
            {
                BoundStatement => ConsoleColor.Cyan,
                BoundExpression => ConsoleColor.Blue,
                _ => ConsoleColor.Yellow
            };

        writer.Write(node);

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
    /// Writes a text-based tree representation of <paramref name="boundNode"/> to <paramref name="textWriter"/>.
    /// </summary>
    /// <param name="boundNode">The <see cref="ExpressionSyntax"/> to represent.</param>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write the output to.</param>
    public static void Print(BoundNode boundNode, TextWriter textWriter) => new BoundTreeWriter(textWriter).Visit(boundNode);
}
