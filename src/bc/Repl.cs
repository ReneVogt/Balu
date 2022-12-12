using Balu.Syntax;
using System.Linq;
using System.Text;
using System;
using System.IO;

namespace Balu;

sealed class Repl
{
    public void Run()
    {
        bool showSyntax = false, showBound = false, showLowered = false;
        VariableDictionary variables = new();
        StringBuilder textBuilder = new();
        Compilation? previous = null;

        while (true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(textBuilder.Length == 0 ? "» " : "· ");
                Console.ResetColor();
                var line = Console.ReadLine();
                string? file = null;
                if (textBuilder.Length == 0)
                {
                    if (line == "#showSyntax")
                    {
                        showSyntax = !showSyntax;
                        Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
                        continue;
                    }

                    if (line == "#showBound")
                    {
                        showBound = !showBound;
                        Console.WriteLine(showBound ? "Showing bound tree." : "Not showing bound tree.");
                        continue;
                    }

                    if (line == "#showLowered")
                    {
                        showLowered = !showLowered;
                        Console.WriteLine(showLowered ? "Showing lowered tree." : "Not showing lowered tree.");
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

                    if (line == "#reset")
                    {
                        previous = null;
                        variables.Clear();
                        continue;
                    }

                    if (line?.StartsWith("#file ") ?? false)
                        file = line[6..];

                    if (string.IsNullOrWhiteSpace(line) || line == "#exit") return;
                }

                SyntaxTree syntaxTree;
                if (file is not null)
                {
                    previous = null;
                    syntaxTree = SyntaxTree.Parse(File.ReadAllText(file));
                }
                else
                {
                    textBuilder.AppendLine(line);
                    var text = textBuilder.ToString();
                    syntaxTree = SyntaxTree.Parse(text);
                    if (!string.IsNullOrWhiteSpace(line) && syntaxTree.Diagnostics.Any()) continue;
                }


                Console.ForegroundColor = ConsoleColor.DarkYellow;
                var compilation = previous?.ContinueWith(syntaxTree) ?? new Compilation(syntaxTree);
                if (showSyntax)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Syntax:");
                    Console.ResetColor();
                    compilation.WriteSyntaxTree(Console.Out);
                }
                if (showBound)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Bound:");
                    Console.ResetColor();
                    compilation.WriteBoundTree(Console.Out);
                }
                if (showLowered)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Lowered:");
                    Console.ResetColor();
                    compilation.WriteLoweredTree(Console.Out);
                }

                var result = compilation.Evaluate(variables);
                Console.ResetColor();
                if (result.Diagnostics.Any())
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        int lineNumber = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var syntaxLine = syntaxTree.Text.Lines[lineNumber];
                        int column = diagnostic.Span.Start - syntaxLine.Start;
                        Console.WriteLine($"[{diagnostic.Id}]({lineNumber + 1}, {column + 1}): {diagnostic.Message}");
                        Console.ResetColor();
                        if (diagnostic.Span.Length > 0)
                        {
                            Console.Write("   ");
                            Console.Write(syntaxTree.Text.ToString(syntaxLine.Start, column));
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(syntaxTree.Text.ToString(diagnostic.Span));
                            Console.ResetColor();
                            Console.WriteLine(syntaxTree.Text.ToString(diagnostic.Span.End, syntaxLine.End - diagnostic.Span.End));
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    previous = compilation;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
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
                textBuilder.Clear();
            }
        }

    }
}
