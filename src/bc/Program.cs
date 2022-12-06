using System;
using System.Linq;
using System.Text;
using Balu;
using Balu.Syntax;
using Balu.Visualization;

internal class Program
{
    private static void Main()
    {
        bool showSyntax = false, showBound = false;
        VariableDictionary variables = new();
        StringBuilder textBuilder = new();

        while (true)
        {
            try
            {
                Console.Write(textBuilder.Length == 0 ? "> " : "| ");
                var line = Console.ReadLine();
                if (textBuilder.Length == 0)
                {
                    if (line == "#syntax")
                    {
                        showSyntax = !showSyntax;
                        Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
                        continue;
                    }

                    if (line == "#bound")
                    {
                        showBound = !showBound;
                        Console.WriteLine(showBound ? "Showing bound tree." : "Not showing bound tree.");
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
                }


                textBuilder.AppendLine(line);
                var text = textBuilder.ToString();
                var syntaxTree = SyntaxTree.Parse(text);
                if (!string.IsNullOrWhiteSpace(line) && syntaxTree.Diagnostics.Any()) continue;

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
                        int lineNumber = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var syntaxLine = syntaxTree.Text.Lines[lineNumber];
                        int column = diagnostic.Span.Start - syntaxLine.Start;
                        Console.WriteLine($"[{diagnostic.Id}]({lineNumber+1}, {column+1}): {diagnostic.Message}");
                        Console.ResetColor();
                        Console.Write("   ");
                        Console.Write(syntaxTree.Text.ToString(syntaxLine.Start, column));
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(syntaxTree.Text.ToString(diagnostic.Span));
                        Console.ResetColor();
                        Console.WriteLine(syntaxTree.Text.ToString(diagnostic.Span.End, syntaxLine.End - diagnostic.Span.End));
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
                textBuilder.Clear();
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