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

    bool showSyntax, showVars, showProgram, writeControlFlow;
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

        if (writeControlFlow)
        {
            using var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!, "graph.dot"));
            compilation.WriteControlFlowGraph(writer);
        }

        var result = compilation.Evaluate(globals);
        Console.ResetColor();
        if (result.Diagnostics.Any())
            Console.Out.WriteDiagnostics(result.Diagnostics);
        else
        {
            previous = compilation;
            SaveSubmission(text);
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
    static void ClearSubmissions() => Directory.Delete(GetSubmissionsPath(), true);
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
    [MetaCommand("writeControlFlow", "Toggles output of the control flow graph to a file (graph.dot).")]
    void WriteControlFlow()
    {
        writeControlFlow = !writeControlFlow;
        Console.WriteLine(writeControlFlow ? "Writeing control flow graph to graph.dot." : "Not writeing control flow graph to graph.dot.");
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
        if (previous is null) return;
        foreach (var symbol in previous.AllVisibleSymbols.OrderBy(symbol => symbol.Name))
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
