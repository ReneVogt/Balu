using System;
using System.IO;
using System.Linq;
using Balu.Syntax;

namespace Balu.Visualization;

public sealed class SyntaxTreePrinter : SyntaxTreeRewriter
{
    readonly TextWriter writer;
    string indent = string.Empty;
    bool last = true;

    SyntaxTreePrinter(TextWriter writer) => this.writer = writer;

    public override SyntaxNode Visit(SyntaxNode node)
    {
        var token = node as SyntaxToken;

        if (token is not null)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                writer.Write(indent);
                writer.WriteColoredText(TreeTexts.Leaf + $"L: {trivia.Kind}", ConsoleColor.DarkGray);
                writer.WriteLine();
            }
        }

        writer.Write(indent);
        writer.WriteColoredText(last && (token is null || token.TrailingTrivia.Length == 0) ? TreeTexts.LastLeaf : TreeTexts.Leaf, ConsoleColor.DarkGray);
        writer.WriteColoredText(node.ToString(), node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan);
        writer.WriteLine();

        if (token is not null)
        {
            for (int i=0; i<token.TrailingTrivia.Length; i++)
            {
                writer.Write(indent);
                writer.WriteColoredText((last && i == token.TrailingTrivia.Length - 1 ? TreeTexts.LastLeaf : TreeTexts.Leaf) + $"T: {token.TrailingTrivia[i].Kind}", ConsoleColor.DarkGray);
                writer.WriteLine();
            }
        }

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

    public static void Print(SyntaxNode syntax, TextWriter textWriter) => new SyntaxTreePrinter(textWriter).Visit(syntax);
}
