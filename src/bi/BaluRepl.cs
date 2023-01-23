using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Balu.Authoring;
using Balu.Diagnostics;
using Balu.Interpretation;
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
    bool showSyntax, showVars, showProgram;
    readonly Interpreter interpreter = new();


    public BaluRepl()
    {
        LoadSubmissions();
    }

    protected override bool IsCompleteSubmission(string text) => string.IsNullOrWhiteSpace(text) || text.EndsWith(Environment.NewLine+Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) || !SyntaxTree.Parse(text).IsLastTokenMissing;

    protected override void EvaluateSubmission(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        interpreter.AddCode(text);
        if (showSyntax)
        {
            Console.Out.WriteColoredText("Syntax:", ConsoleColor.Yellow);
            Console.Out.WriteLine();
            interpreter.Compilation.WriteSyntaxTrees(Console.Out);
        }
        if (showProgram)
        {
            Console.Out.WriteColoredText("Program:", ConsoleColor.Yellow);
            Console.Out.WriteLine();
            interpreter.Compilation.WriteBoundGlobalTree(Console.Out);
        }

        Console.ForegroundColor = ConsoleColor.White;
        var diagnostics = interpreter.Execute();
        Console.ResetColor();
        Console.Out.WriteDiagnostics(diagnostics);
        if (diagnostics.HasErrors()) return;
        
        if (interpreter.Result is not null)
        {
            Console.Out.WriteColoredText("Result: ", ConsoleColor.Yellow);
            if (interpreter.Result is string s)
                Console.Out.WriteColoredText($"\"{s.EscapeString()}\"", ConsoleColor.Magenta);
            else
                Console.Out.WriteColoredText(interpreter.Result.ToString(), ConsoleColor.Magenta);
            Console.Out.WriteLine();
        }

        SaveSubmission(text);
        if (showVars)
        {
            Console.Out.WriteColoredText("Variables:", ConsoleColor.Yellow);
            Console.Out.WriteLine();
            foreach (var (global, value) in interpreter.GlobalVariables)
            {
                Console.Out.WriteIdentifier(global.Name);
                Console.Out.WritePunctuation("(");
                Console.Out.WriteIdentifier(global.Type.Name);
                Console.Out.WritePunctuation(")");
                Console.Out.WriteSpace();
                Console.Out.WritePunctuation("=");
                Console.Out.WriteSpace();
                Console.Out.Write(value.ToString() ?? "<null>");
                Console.Out.WriteLine();
            }
        }

        Console.ResetColor();
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
        foreach (var text in files.Select(File.ReadAllText)) { AddToHistory(text); EvaluateSubmission(text);}
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
        var syntaxTree = state as SyntaxTree ?? SyntaxTree.Parse(string.Join(Environment.NewLine, lines));

        var line = syntaxTree.Text.Lines[lineIndex];
        var classifiedSpans = Classifier.Classify(syntaxTree, line.Span);
        int width = 0;
        foreach (var classifiedSpan in classifiedSpans)
        {
            var color = classifiedSpan.Classification switch
            {
                Classification.Keyword => ConsoleColor.Blue,
                Classification.Identifier => ConsoleColor.DarkYellow,
                Classification.Number => ConsoleColor.Cyan,
                Classification.String => ConsoleColor.Magenta,
                                         Classification.Comment => ConsoleColor.Green,
                Classification.Bad => ConsoleColor.Red,
                _ => ConsoleColor.DarkGray
            };

            Console.Out.WriteColoredText(syntaxTree.Text.ToString(classifiedSpan.Span), color);
            width += classifiedSpan.Span.Length;
        }
        Console.Out.Write(new string(' ', Console.WindowWidth-2-width));
        return syntaxTree;
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
        interpreter.Reset();
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
        foreach (var symbol in interpreter.VisibleSymbols.OrderBy(symbol => symbol.Name))
        {
            symbol.WriteTo(Console.Out);
            Console.Out.WriteLine();
        }
    }
    [MetaCommand("dump", "Shows the compiled function with the given name.")]
    void Dump(string functionName)
    {
        var function = interpreter.VisibleSymbols.OfType<FunctionSymbol>().SingleOrDefault(function => function.Name == functionName);
        if (function is null)
        {
            Console.Error.WriteColoredText($"Error: Function '{functionName}' does not exist.{Environment.NewLine}", ConsoleColor.Red);
            return;
        }

        interpreter.Compilation.WriteBoundFunctionTree(Console.Out, function);
    }
    [MetaCommand("emit", "Emits the current script as assembly to the specified location.")]
    void Emit(string path)
    {
        var diagnostics = interpreter.Emit(path);
        Console.Error.WriteDiagnostics(diagnostics);
    }
    [MetaCommand("emitd", "Emits the current script with debug symbols as assembly to the specified location.")]
    void EmitDebug(string path)
    {
        var diagnostics = interpreter.Emit(path, Path.ChangeExtension(path, ".pdb"));
        Console.Error.WriteDiagnostics(diagnostics);
    }
    [MetaCommand("graph", "Writes the control flow graph of a function as a GraphViz dot representation to the specified path.")]
    [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "...")]
    void Graph(string functionName, string path)
    {
        var function = interpreter.VisibleSymbols.OfType<FunctionSymbol>().SingleOrDefault(function => function.Name == functionName);
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
            interpreter.Compilation.WriteControlFlowGraph(writer, function);
            Console.Out.WritePunctuation($"Successfully wrote control flow graph of function '{functionName}' to file '{file}'.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteColoredText($"Error: Could not write control flow graph of function '{functionName}' to file '{file}': {exception.Message}", ConsoleColor.Red);
        }

        Console.Out.WriteLine();
    }
}
