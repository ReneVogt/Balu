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
using Balu.Text;
using Balu.Visualization;

#pragma warning disable CA1303

namespace Balu.Interactive;

sealed class BaluRepl : Repl, IDisposable
{
    bool showVars;
    readonly Interpreter interpreter = new(
    [
        Path.Combine(AppContext.BaseDirectory, "reference-assemblies", "System.Runtime.dll"),
        Path.Combine(AppContext.BaseDirectory, "reference-assemblies", "System.Runtime.Extensions.dll"),
        Path.Combine(AppContext.BaseDirectory, "reference-assemblies", "System.Console.dll")
    ]) {Out = Console.Out, Error = Console.Error};

    public void Dispose() => interpreter.Dispose();

    protected override bool IsCompleteSubmission(string text) => string.IsNullOrWhiteSpace(text) || text.EndsWith(Environment.NewLine+Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) || !SyntaxTree.Parse(text).IsLastTokenMissing;

    protected override void EvaluateSubmission(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        var diagnostics = interpreter.Execute(text);
        Console.ResetColor();
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

    protected override object? RenderLine(IReadOnlyList<string> lines, int lineIndex, int start, int length, TextWriter writer, object? state)
    {
        var syntaxTree = state as SyntaxTree ?? SyntaxTree.Parse(string.Join(Environment.NewLine, lines));

        var line = syntaxTree.Text.Lines[lineIndex];
        var classifiedSpans = Classifier.Classify(syntaxTree, new TextSpan(line.Start + start, length));
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

            writer.WriteColoredText(syntaxTree.Text.ToString(classifiedSpan.Span), color);
        }
        return syntaxTree;
    }

    [MetaCommand("showSyntax", "Toggles display of the syntax tree.")]
    void ShowSyntax()
    {
        interpreter.WriteSyntax = !interpreter.WriteSyntax;
        Console.WriteLine(interpreter.WriteSyntax ? "Showing syntax tree." : "Not showing syntax tree.");
    }
    [MetaCommand("showProgram", "Toggles display of the bound program.")]
    void ShowProgram()
    {
        interpreter.WriteProgram = !interpreter.WriteProgram;
        Console.WriteLine(interpreter.WriteProgram ? "Showing program tree." : "Not showing program tree.");
    }
    [MetaCommand("showVars", "Toggles display of variables' content.")]
    void ShowVariables()
    {
        showVars = !showVars;
        Console.WriteLine(showVars ? "Showing globals after evaluationn." : "Not showing globals after evaluation.");
    }
    [MetaCommand("cls", "Clears the screen.")]
    static void ClearScreen() => Console.Clear();
    [MetaCommand("reset", "Resets the current interpreter session.")]
    void Reset() => interpreter.Reset();
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
