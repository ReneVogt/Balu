using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Balu;
using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Visualization;
using Mono.Options;
#pragma warning disable CA1031 // Main catches all exceptions

sealed class Program
{
    const int Success = 0;
    const int CompilationError = 1;
    const int InvocationError = 2;
    const int ToolError = 3;

    readonly TextWriter output;
    readonly TextWriter error;
    bool quiet;

    internal Program(TextWriter output, TextWriter error)
    {
        this.output = TextWriter.Synchronized(output);
        this.error = TextWriter.Synchronized(error);
    }

    public static int Main(string[] args) => new Program(Console.Out, Console.Error).Run(args);

    internal int Run(string[] args)
    {
        List<string> references = [];
        string outputPath = string.Empty;
        string symbolPath = string.Empty;
        string moduleName = string.Empty;
        List<string> sourcePaths = [];
        bool helpRequested = false;
        bool? debug = null;

        var options = new OptionSet
        {
            "Usage: bc <source-paths> [options]",
            { "r=", "The {path} of an assembly to reference.", v => references.Add(v) },
            { "o=", "The output {path} of the assembly to create.", v => outputPath = v },
            { "s=", "The optional symbol {path} of the pdb to create.", v => symbolPath = v },
            { "debug=", "Whether to emit debugger-friendly IL.", (bool v) => debug = v },
            { "m=", "The module {name} of the assembly to create.", v => moduleName = v },
            { "q", _ => quiet = true },
            { "?|h|help", "Shows help.", _ => helpRequested = true }
        };

        quiet = args.TakeWhile(argument => argument != "--").Any(IsQuietOption);
        try
        {
            var optionTerminatorIndex = Array.IndexOf(args, "--");
            var argumentsToParse = optionTerminatorIndex < 0 ? args : args[..optionTerminatorIndex];
            sourcePaths.AddRange(options.Parse(argumentsToParse));
            var unknownOption = sourcePaths.FirstOrDefault(IsUnknownOption);
            if (unknownOption is not null)
            {
                LogError($"Unknown option '{unknownOption}'.");
                return InvocationError;
            }

            if (optionTerminatorIndex >= 0)
                sourcePaths.AddRange(args[(optionTerminatorIndex + 1)..]);
        }
        catch (OptionException parseError)
        {
            LogError(parseError.Message);
            return InvocationError;
        }

        if (helpRequested)
        {
            options.WriteOptionDescriptions(output);
            return Success;
        }

        LogInfo($"Balu compiler v{Assembly.GetExecutingAssembly().GetName().Version}");

        try
        {
            if (sourcePaths.Count == 0)
            {
                LogError("need at least one source file.");
                return InvocationError;
            }

            if (string.IsNullOrWhiteSpace(outputPath))
                outputPath = Path.ChangeExtension(sourcePaths[0], ".dll");

            if (string.IsNullOrWhiteSpace(moduleName))
                moduleName = Path.GetFileNameWithoutExtension(outputPath);

            outputPath = Path.GetFullPath(outputPath);

            // Keep command-line order for binding; parsing progress still reflects parallel execution.
            var syntaxTrees = sourcePaths.AsParallel().AsOrdered().Select(Parse).ToArray();
            var compilation = Compilation.Create(syntaxTrees);
            LogInfo(
                $"Emitting assembly '{outputPath}'{(string.IsNullOrWhiteSpace(symbolPath) ? string.Empty : $" and symbol file '{symbolPath}'")}.");
            var diagnostics = compilation.Emit(moduleName, [.. references], outputPath, symbolPath, debug ?? !string.IsNullOrWhiteSpace(symbolPath));
            LogDiagnostics(diagnostics);
            LogInfo("Done.");
            return diagnostics.HasErrors() ? CompilationError : Success;
        }
        catch (AggregateException aggregateError)
        {
            foreach (var innerError in aggregateError.Flatten().InnerExceptions)
                LogError(innerError.Message);
            return ToolError;
        }
        catch (Exception error)
        {
            LogError(error.Message);
            return ToolError;
        }
    }

    static bool IsQuietOption(string argument) => argument is "-q" or "/q" or "--q";
    static bool IsUnknownOption(string argument) =>
        argument.StartsWith('-') || OperatingSystem.IsWindows() && argument.StartsWith('/');

    SyntaxTree Parse(string path)
    {
        LogInfo($"Compiling '{path}'...");
        return SyntaxTree.Load(Path.GetFullPath(path));
    }

    void LogInfo(string message)
    {
        if (!quiet)
            output.WriteLine(message);
    }

    void LogError(string message)
    {
        if (quiet) return;
        error.WriteColoredText("bc: error: ", ConsoleColor.Red);
        error.WriteColoredText(message, ConsoleColor.Red);
        error.WriteLine();
    }
    void LogDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        if (!quiet)
            error.WriteDiagnostics(diagnostics);
    }
}
