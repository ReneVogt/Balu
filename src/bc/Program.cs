using System;
using System.Linq;
using Balu;
using Balu.Syntax;
using Balu.Visualization;

internal class Program
{
    private static void Main()
    {
        bool showSyntax = false;

        while (true)
        {
            try
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == "#tree")
                {
                    showSyntax = !showSyntax;
                    Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
                    continue;
                }
                if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || line == "#exit") return;

                var syntaxTree = SyntaxTree.Parse(line);
                if (showSyntax)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    SyntaxTreePrinter.Print(syntaxTree.Root, Console.Out);
                    Console.ResetColor();
                }

                var result = Compilation.Evaluate(syntaxTree);
                if (result.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Join(Environment.NewLine, result.Diagnostics));
                    Console.ResetColor();
                }
                else
                    Console.WriteLine(result.Value);

                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
            }
        }
    }
}