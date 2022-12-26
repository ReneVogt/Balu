using System;
using System.IO;
using System.Linq;
using Balu.Syntax;
using Balu.Visualization;
// ReSharper disable UnusedMember.Local

#pragma warning disable IDE0051
#pragma warning disable IDE0040
#pragma warning disable CA1303

namespace Balu.Interactive;

sealed class BaluRepl : Repl
{
    readonly VariableDictionary globals = new();

    bool showSyntax, showVars, showProgram;
    Compilation? previous;

    protected override bool IsCompleteSubmission(string text) => string.IsNullOrWhiteSpace(text) || text.EndsWith(Environment.NewLine+Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) || !SyntaxTree.Parse(text).IsLastTokenMissing;

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
            compilation.WriteSyntaxTrees(Console.Out);
        }
        if (showProgram)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Program:");
            Console.ResetColor();
            compilation.WriteProgramTree(Console.Out);
        }


        using var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!, "graph.dot"));
        compilation.WriteControlFlowGraph(writer);

        var result = compilation.Evaluate(globals);
        Console.ResetColor();
        Console.WriteLine();
        if (result.Diagnostics.Any())
            Console.Out.WriteDiagnostics(result.Diagnostics);
        else
        {
            previous = compilation;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Result: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(result.Value is string s ? $"\"{s.EscapeString()}\"" : result.Value);
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

    [MetaCommand("showSyntax", "Toggles display of the syntax tree.")]
    void ShowSyntax()
    {
        showSyntax = !showSyntax;
        Console.WriteLine(showSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
    }
    [MetaCommand("showProgram", "Toggles display of the bound program.")]
    void ShowProgram()
    {
        showProgram = !showProgram;
        Console.WriteLine(showProgram ? "Showing program tree." : "Not showing program tree.");
    }
    [MetaCommand("showVars", "Toggles display of variables' content.")]
    void ShowVariables()
    {
        showVars = !showVars;
        Console.WriteLine(showVars ? "Showing globals after evaluationn." : "Not showing globals after evaluation.");
    }
    [MetaCommand("cls", "Clears the screen.")]
    static void ClearScreen() => Console.Clear();
    [MetaCommand("reset", "Resets the submission cache.")]
    void Reset()
    {
        previous = null;
        globals.Clear();
    }
}
