using System;
using System.Linq;
using Balu;
using Balu.Binding;
using Balu.Syntax;
using Balu.Visualization;

bool showSyntax = false, showBound = false;

while (true)
{
    try
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line == "#syntax")
        {
            showSyntax = !showSyntax;
            Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
            continue;
        }
        if (line == "#bound")
        {
            showBound = !showBound;
            Console.WriteLine(showBound ? "Showing bound tree." : "Not showing boud tree.");
            continue;
        }

        if (line == "#cls")
        {
            Console.Clear();
            continue;
        }

        if (string.IsNullOrWhiteSpace(line) || line == "#exit") return;

        var parser = new Parser(line);
        var syntaxTree = parser.Parse();
        if (showSyntax)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            SyntaxTreePrinter.Print(syntaxTree.Root, Console.Out);
            Console.ResetColor();
        }

        var boundTree = Binder.Bind(syntaxTree.Root);
        if (showBound)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            BoundTreePrinter.Print(boundTree.Root, Console.Out);
            Console.ResetColor();
        }
        
        if (boundTree.Diagnostics.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.Join(Environment.NewLine, boundTree.Diagnostics));
            Console.ResetColor();
        }
        else
            Console.WriteLine(new Evaluator(syntaxTree.Root).Evaluate());

        Console.WriteLine();
    }
    catch (Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception);
        Console.ResetColor();
    }
}