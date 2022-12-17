using Balu.Syntax;
using System.Linq;
using System;
#pragma warning disable CA1303

namespace Balu;

sealed class BaluRepl : Repl
{
    readonly VariableDictionary globals = new();

    bool showSyntax, showBound, showLowered, showVars;
    Compilation? previous;

    protected override bool IsCompleteSubmission(string text) => string.IsNullOrWhiteSpace(text) || text.EndsWith(Environment.NewLine+Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) || !SyntaxTree.Parse(text).IsLastTokenMissing;

    protected override void EvaluateMetaCommand(string text)
    {
        switch (text)
        {
            case "#showSyntax":
                showSyntax = !showSyntax;
                Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
                break;
            case "#showBound":
                showBound = !showBound;
                Console.WriteLine(showBound ? "Showing bound tree." : "Not showing bound tree.");
                break;
            case "#showLowered":
                showLowered = !showLowered;
                Console.WriteLine(showLowered ? "Showing lowered tree." : "Not showing lowered tree.");
                break;
            case "#showVars":
                showVars = !showVars;
                Console.WriteLine(showVars ? "Showing globals after evaluationn." : "Not showing globals after evaluation.");
                break;
            case "#cls":
                Console.Clear();
                break;
            case "#reset":
                previous = null;
                globals.Clear();
                break;
            case "#clearHistory":
                ClearHistory();
                break;
            default:
                base.EvaluateMetaCommand(text);
                break;
        }
    }
    protected override void EvaluateSubmission(string text)
    {
        SyntaxTree syntaxTree = SyntaxTree.Parse(text);
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

        var result = compilation.Evaluate(globals);
        Console.ResetColor();
        Console.WriteLine();
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
                    Console.WriteLine(syntaxTree.Text.ToString(diagnostic.Span.End, Math.Max(0, syntaxLine.End - diagnostic.Span.End)));
                    Console.ResetColor();
                }
            }
        }
        else
        {
            previous = compilation;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Result: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(result.Value);
            if (showVars)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Variables:");
                Console.ResetColor();
                if (globals.Any())
                    Console.WriteLine(string.Join(Environment.NewLine, globals.Select(kvp => $"{kvp.Key.Name}({kvp.Key.Type.Name}): {kvp.Value}")));
            }
            Console.ResetColor();
        }
    }

    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens)
        {
            Console.ForegroundColor = token.Kind switch
            {
                >= SyntaxKind.TrueKeyword and < SyntaxKind.CompilationUnit => ConsoleColor.Blue,
                SyntaxKind.IdentifierToken => ConsoleColor.DarkYellow,
                SyntaxKind.NumberToken => ConsoleColor.Cyan,
                SyntaxKind.StringToken => ConsoleColor.Magenta,

                _ => ConsoleColor.DarkGray
            };

            Console.Write(token.Text);
            Console.ResetColor();
        }

    }
}
