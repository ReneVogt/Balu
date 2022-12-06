using System;
using System.Linq;
using Balu;
using Balu.Syntax;
using Balu.Visualization;

internal class Program
{
    private static void Main()
    {
        bool showSyntax = false, showBound = false;
        VariableDictionary variables = new();

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
                    Console.WriteLine(showBound? "Showing bound tree." : "Not showing bound tree.");
                    continue;
                }
                if (line == "#clear")
                {
                    Console.WriteLine("Clearing variable store.");
                    variables.Clear();
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

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                var result = Compilation.Evaluate(syntaxTree, variables, Console.Out, showBoundTree: showBound);
                Console.ResetColor();
                if (result.Diagnostics.Any())
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        int lineNumber = syntaxTree.Text.GetLineIndex(diagnostic.TextSpan.Start);
                        int column = diagnostic.TextSpan.Start - syntaxTree.Text.Lines[lineNumber].Start;
                        Console.WriteLine($"[{diagnostic.Id}]({lineNumber}, {column}): {diagnostic.Message}");
                        Console.ResetColor();
                        Console.Write("   ");
                        Console.Write(line[..diagnostic.TextSpan.Start]);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(line.Substring(diagnostic.TextSpan.Start, diagnostic.TextSpan.Length));
                        Console.ResetColor();
                        Console.WriteLine(line[diagnostic.TextSpan.End..]);
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine(result.Value);
                    if (variables.Any())
                        Console.WriteLine(string.Join(Environment.NewLine, variables.Select(kvp => $"{kvp.Key.Name}({kvp.Key.Type.Name}): {kvp.Value}")));
                }

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