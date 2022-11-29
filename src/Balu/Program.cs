using System;
using System.Linq;
using Balu;

while (true)
{
    try
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line)) return;
        var parser = new Parser(line);
        var expression = parser.Parse();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        PrettyPrint(expression.Root);
        if (parser.Diagnostics.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Join(Environment.NewLine, parser.Diagnostics));
            Console.ResetColor();
        }

        Console.WriteLine();

        static void PrettyPrint(SyntaxNode node, string indent = "", bool last = true)
        {
            var marker = last ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.WriteLine(node);

            indent += last ? "    " : "│   ";
            var children = node.Children.ToArray();
            for(int i = 0; i<children.Length -1; i++)
                PrettyPrint(children[i], indent, false);
            if (children.Length > 0)
                PrettyPrint(children[^1], indent);
        }
    }
    catch (Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception);
        Console.ResetColor();
    }
}