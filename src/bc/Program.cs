using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Balu;
using Balu.Syntax;
using Balu.Visualization;
using Mono.Options;
#pragma warning disable CA1031 // Main catches all exceptions

sealed class Program
{
    static bool quiet;
    public static int Main(string[] args)
    {
        List<string> references = new();
        string outputPath = string.Empty;
        string symbolPath = string.Empty;
        string moduleName = string.Empty;
        List<string> sourcePaths = new();
        bool helpRequested = false;

        var options = new OptionSet
        {
            "Usage: bc <soure-paths> [options]",
            { "r=", "The {path} of an assembly to reference.", v => references.Add(v) },
            { "o=", "The output {path} of the assembly to create.", v => outputPath = v },
            { "s=", "The optional symbol {path} of the pdb to create.", v => symbolPath = v },
            { "m=", "The module {name} of the assembly to create.", v => moduleName = v },
            { "q", _ => quiet = true },
            { "<>", v => sourcePaths.Add(v) },
            { "?|h|help", "Shows help.", _ => helpRequested = true }
        };

        options.Parse(args);

        if (helpRequested)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        LogInfo($"Balu compiler v{Assembly.GetExecutingAssembly().GetName().Version}");

        try
        {
            if (sourcePaths.Count == 0)
            {
                LogError("Error: need at least one source file.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = Path.ChangeExtension(sourcePaths[0], ".dll");

            if (string.IsNullOrWhiteSpace(moduleName))
                moduleName = Path.GetFileNameWithoutExtension(outputPath);

            outputPath = Path.GetFullPath(outputPath);

            var syntaxTrees = sourcePaths.AsParallel().Select(Parse).ToArray();
            var compilation = Compilation.Create(syntaxTrees);
            LogInfo(
                $"Emitting assembly '{outputPath}'{(string.IsNullOrWhiteSpace(symbolPath) ? string.Empty : $" and symbol file '{symbolPath}'")}.");
            var diagnostics = compilation.Emit(moduleName, references.ToArray(), outputPath, symbolPath);
            LogDiagnostics(diagnostics);
            LogInfo("Done.");
            return diagnostics.Any() ? 1 : 0;
        }
        catch (Exception error)
        {
            LogError(error.Message);
            return 1;
        }
    }

    static SyntaxTree Parse(string path)
    {
        LogInfo($"Compiling '{path}'...");
        return SyntaxTree.Load(Path.GetFullPath(path));
    }

    static void LogInfo(string message)
    {
        if (!quiet)
            Console.WriteLine(message);
    }

    static void LogError(string message)
    {
        if (quiet) return;
        Console.Error.WriteColoredText(message, ConsoleColor.Red);
        Console.Error.WriteLine();
    }
    static void LogDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        if (!quiet) 
            Console.Error.WriteDiagnostics(diagnostics);
    }
}
