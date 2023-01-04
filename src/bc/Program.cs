using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balu;
using Balu.Syntax;
using Balu.Visualization;
using Mono.Options;

#pragma warning disable CA1031 // Main catches all exceptions

sealed class Program
{
    public static int Main(string[] args)
    {
        List<string> references = new();
        string outputPath = string.Empty;
        string moduleName = string.Empty;
        List<string> sourcePaths = new();
        bool helpRequested = false;

        var options = new OptionSet
        {
            "Usage: bc <soure-paths> [options]",
            { "r=", "The {path} of an assembly to reference.", v => references.Add(v) },
            { "o=", "The output {path} of the assembly to create.", v => outputPath = v },
            { "m=", "The module {name} of the assembly to create.", v => moduleName = v },
            { "<>", v => sourcePaths.Add(v) },
            { "?|h|help", "Shows help.", _ => helpRequested = true }
        };

        options.Parse(args);

        if (helpRequested)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        
        try
        {
            if (sourcePaths.Count == 0)
            {
                Console.Error.WriteColoredText("Error: need at least one source file.", ConsoleColor.Red);
                Console.Error.WriteLine();
                return 1;
            }

            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = Path.ChangeExtension(sourcePaths[0], ".exe");

            if (string.IsNullOrWhiteSpace(moduleName))
                moduleName = Path.GetFileNameWithoutExtension(outputPath);

            outputPath = Path.GetFullPath(outputPath);

            var syntaxTrees = sourcePaths.Select(path => SyntaxTree.Load(Path.GetFullPath(path))).ToArray();
            var compilation = Compilation.Create(syntaxTrees);
            var diagnostics = compilation.Emit(moduleName, references.ToArray(), outputPath);
            if (!diagnostics.Any()) return 0;

            Console.Error.WriteDiagnostics(diagnostics);
            return 1;
        }
        catch (Exception error)
        {
            Console.Error.WriteColoredText($"Fatal error: {error.Message}.", ConsoleColor.Red);
            Console.Error.WriteLine();
            return 1;
        }
    }
}
