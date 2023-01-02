using System;
using System.IO;
using System.Linq;
using Balu.Symbols;
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

    public BaluRepl()
    {
        LoadSubmissions();
    }

    protected override bool IsCompleteSubmission(string text) => string.IsNullOrWhiteSpace(text) || text.EndsWith(Environment.NewLine+Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) || !SyntaxTree.Parse(text).IsLastTokenMissing;

    protected override void EvaluateSubmission(string text)
    {
        SyntaxTree syntaxTree = SyntaxTree.Parse(text);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        var compilation = Compilation.CreateScript(previous, syntaxTree);
        if (showSyntax)
        {
            Console.Out.WriteColoredText("Syntax:", ConsoleColor.Yellow);
            Console.Out.WriteLine();
            compilation.WriteSyntaxTrees(Console.Out);
        }
        if (showProgram)
        {
            Console.Out.WriteColoredText("Program:", ConsoleColor.Yellow);
            Console.Out.WriteLine();
            compilation.WriteProgramTree(Console.Out);
        }

        var result = compilation.Evaluate(globals);
        Console.ResetColor();
        if (result.Diagnostics.Any())
            Console.Out.WriteDiagnostics(result.Diagnostics);
        else
        {
            if (result.Value is not null)
            {
                Console.Out.WriteColoredText("Result: ", ConsoleColor.Yellow);
                if (result.Value is string s)
                    Console.Out.WriteColoredText($"\"{s.EscapeString()}\"", ConsoleColor.Magenta);
                else
                    Console.Out.WriteColoredText(result.Value.ToString(), ConsoleColor.Magenta);
                Console.Out.WriteLine();
            }

            previous = compilation;
            SaveSubmission(text);
            if (showVars)
            {
                Console.Out.WriteColoredText("Variables:", ConsoleColor.Yellow);
                Console.Out.WriteLine();
                foreach (var element in globals)
                {
                    Console.Out.WriteIdentifier(element.Key.Name);
                    Console.Out.WritePunctuation("(");
                    Console.Out.WriteIdentifier(element.Key.Type.Name);
                    Console.Out.WritePunctuation(")");
                    Console.Out.WriteSpace();
                    Console.Out.WritePunctuation("=");
                    Console.Out.WriteSpace();
                    Console.Out.Write(element.Value?.ToString() ?? "<null>");
                    Console.Out.WriteLine();
                }
            }

            Console.ResetColor();
        }
    }

    bool loadingSubmission;
    void LoadSubmissions()
    {
        var submissionsPath = GetSubmissionsPath();
        if (!Directory.Exists(submissionsPath)) return;
        var files = Directory.GetFiles(submissionsPath).OrderBy(f => f).ToArray();
        if (files.Length == 0) return;
        loadingSubmission = true;
        Console.Out.WritePunctuation($"Loading {files.Length} submissions...{Environment.NewLine}");
        foreach (var text in files.Select(File.ReadAllText)) EvaluateSubmission(text);
        loadingSubmission = false;
    }
    void SaveSubmission(string text)
    {
        if (loadingSubmission) return;
        var submissionFolder = GetSubmissionsPath();
        Directory.CreateDirectory(submissionFolder);
        var existingCount = Directory.GetFiles(submissionFolder).Length;
        var name = $"submission{existingCount:0000}";
        var fileName = Path.Combine(submissionFolder, name);
        File.WriteAllText(fileName, text);
    }
    static void ClearSubmissions()
    {
        var path = GetSubmissionsPath();
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
    static string GetSubmissionsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var submissionFolder = Path.Combine(localAppData, "Balu", "Submissions");
        return submissionFolder;
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
        ClearSubmissions();
    }
    [MetaCommand("load", "Loads a script file.")]
    void Load(string path)
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
        {
            Console.Error.WriteColoredText($"Error: file '{path}' does not exist.{Environment.NewLine}", ConsoleColor.Red);
            return;
        }

        EvaluateSubmission(File.ReadAllText(path));
    }
    [MetaCommand("ls", "Lists all symbols.")]
    void ListSymbols()
    {
        var compilation = previous ?? Compilation.CreateScript(null);
        foreach (var symbol in compilation.AllVisibleSymbols.OrderBy(symbol => symbol.Name))
        {
            symbol.WriteTo(Console.Out);
            Console.Out.WriteLine();
        }
    }
    [MetaCommand("dump", "Shows the compiled function with the given name.")]
    void Dump(string functionName)
    {
        var compilation = previous ?? Compilation.CreateScript(null);
        var function = compilation.AllVisibleSymbols.OfType<FunctionSymbol>().SingleOrDefault(function => function.Name == functionName);
        if (function is null)
        {
            Console.Error.WriteColoredText($"Error: Function '{functionName}' does not exist.{Environment.NewLine}", ConsoleColor.Red);
            return;
        }

        compilation.WriteTree(Console.Out, function);
    }
}
