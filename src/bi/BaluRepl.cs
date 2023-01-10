using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using Balu.Visualization;
// ReSharper disable UnusedMember.Local

#pragma warning disable IDE0051
#pragma warning disable IDE0040
#pragma warning disable CA1303

namespace Balu.Interactive;

sealed class BaluRepl : Repl
{
    sealed class RenderState
    {
        public SourceText Text { get; }
        public ImmutableArray<SyntaxToken> Tokens { get;}
        public RenderState(SourceText text, ImmutableArray<SyntaxToken> tokens)
        {
            Text = text;
            Tokens = tokens;
        }
    }
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
            compilation.WriteBoundGlobalTree(Console.Out);
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

    protected override object? RenderLine(IReadOnlyList<string> lines, int lineIndex, object? state)
    {
        if (state is not RenderState renderState)
        {
            var text = string.Join(Environment.NewLine, lines);
            var sourceText = SourceText.From(text);
            renderState = new (sourceText, SyntaxTree.ParseTokens(sourceText));
            state = renderState;
        }

        var line = renderState.Text.Lines[lineIndex];
        int width = 0;

        foreach (var token in renderState.Tokens)
        {
            if (!line.Span.OverlapsWith(token.Span)) continue;

            var color = token.Kind.IsKeyword()
                            ? ConsoleColor.Blue
                            : token.Kind.IsComment()
                                ? ConsoleColor.Green
                                : token.Kind switch
                                {
                                    SyntaxKind.IdentifierToken => ConsoleColor.DarkYellow, SyntaxKind.NumberToken => ConsoleColor.Cyan,
                                    SyntaxKind.StringToken => ConsoleColor.Magenta,
                                    _ => ConsoleColor.DarkGray
                                };

            var start = Math.Max(token.Span.Start, line.Span.Start);
            var end = Math.Min(token.Span.End, line.Span.End);
            var span = new TextSpan(start, end-start);
            var text = renderState.Text.ToString(span);
            width += text.Length;
            Console.Out.WriteColoredText(text, color);
        }
        Console.Out.Write(new string(' ', Console.WindowWidth-2-width));
        return state;
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

        compilation.WriteBoundFunctionTree(Console.Out, function);
    }
    [MetaCommand("graph", "Writes the control flow graph of a function as a GraphViz dot representation to the specified path.")]
    [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "...")]
    void Graph(string functionName, string path)
    {
        var compilation = previous ?? Compilation.CreateScript(null);
        var function = compilation.AllVisibleSymbols.OfType<FunctionSymbol>().SingleOrDefault(function => function.Name == functionName);
        if (function is null)
        {
            Console.Error.WriteColoredText($"Error: Function '{functionName}' does not exist.{Environment.NewLine}", ConsoleColor.Red);
            return;
        }

        string file = path;
        try
        {
            file = Path.GetFullPath(path);
            using var writer = new StreamWriter(file);
            compilation.WriteControlFlowGraph(writer, function);
            Console.Out.WritePunctuation($"Successfully wrote control flow graph of function '{functionName}' to file '{file}'.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteColoredText($"Error: Could not write control flow graph of function '{functionName}' to file '{file}': {exception.Message}", ConsoleColor.Red);
        }

        Console.Out.WriteLine();
    }
}
