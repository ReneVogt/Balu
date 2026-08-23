using System;
using System.Collections.Generic;
using System.IO;
using Balu.Syntax;

namespace Balu.Visualization;

public sealed class SyntaxTreePrinter
{
    SyntaxTreePrinter() { }

    public static void Print(SyntaxNode syntax, TextWriter writer)
    {
        _ = syntax ?? throw new ArgumentNullException(nameof(syntax));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var stack = new Stack<(SyntaxNode node, string indent, bool isLast)>();
        stack.Push((syntax, string.Empty, true));
        while (stack.Count > 0)
        {
            var (node, indent, isLast) = stack.Pop();
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
            writer.WriteColoredText(isLast && (token is null || token.TrailingTrivia.Length == 0) ? TreeTexts.LastLeaf : TreeTexts.Leaf, ConsoleColor.DarkGray);
            writer.WriteColoredText(node.ToString(), node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan);
            writer.WriteLine();

            if (token is not null)
            {
                for (int i=0; i<token.TrailingTrivia.Length; i++)
                {
                    writer.Write(indent);
                    writer.WriteColoredText((isLast && i == token.TrailingTrivia.Length - 1 ? TreeTexts.LastLeaf : TreeTexts.Leaf) + $"T: {token.TrailingTrivia[i].Kind}", ConsoleColor.DarkGray);
                    writer.WriteLine();
                }
            }

            var childIndent = indent + (isLast ? TreeTexts.Indent : TreeTexts.Branch);
            for (var i = node.ChildrenCount - 1; i >= 0; i--)
                stack.Push((node.GetChild(i), childIndent, i == node.ChildrenCount - 1));
        }
    }
}
