using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Balu.Binding;
using Balu.Diagnostics;
using Balu.Emit;
using Balu.Lowering;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Visualization;
using Binder = Balu.Binding.Binder;

#pragma warning disable CA1724
namespace Balu;

public sealed class Compilation
{
    static readonly StringComparer pathComparer =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    readonly Compilation? previous;
    BoundGlobalScope? globalScope;
    BoundProgram? program;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (globalScope is null)
            {
                var scope = Binder.BindGlobalScope(IsScript, previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref globalScope, scope, null);
            }

            return globalScope;
        }
    }
    internal BoundProgram Program
    {
        get
        {
            if (program is null)
            {
                var p = Binder.BindProgram(IsScript, previous?.Program, GlobalScope);
                Interlocked.CompareExchange(ref program, p, null);
            }

            return program;
        }
    }

    public bool IsScript { get; }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public ImmutableArray<Diagnostic> Diagnostics => Program.Diagnostics;

    public FunctionSymbol? MainFunction => GlobalScope.EntryPoint;
    public ImmutableArray<Symbol> VisibleSymbols => GlobalScope.VisibleSymbols;
    public ImmutableArray<Symbol> AllSymbols => GlobalScope.AllSymbols;

    Compilation(bool isScript, Compilation? previous, params SyntaxTree[] syntaxTrees)
    {
        if (previous?.Diagnostics.HasErrors() == true)
            throw new ArgumentException("A compilation can only be continued if it does not contain any errors.", nameof(previous));
        this.previous = previous;
        SyntaxTrees = syntaxTrees.DefaultIfEmpty(SyntaxTree.Parse(string.Empty)).ToImmutableArray();
        IsScript = isScript;
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath) =>
        Emit(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty);
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

        var pathDiagnostics = ValidateEmitPaths(outputPath, symbolPath, out var canonicalOutputPath, out var canonicalSymbolPath);
        if (pathDiagnostics.HasErrors()) return pathDiagnostics;
        if (Program.Diagnostics.HasErrors()) return Program.Diagnostics;

        using var referenceSet = new EmitReferenceSet(references);
        return EmitToFiles(moduleName, referenceSet, canonicalOutputPath, canonicalSymbolPath, initializedGlobalVariables);
    }
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath) =>
        EmitWithReferenceSet(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty);
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

        var pathDiagnostics = ValidateEmitPaths(outputPath, symbolPath, out var canonicalOutputPath, out var canonicalSymbolPath);
        if (pathDiagnostics.HasErrors()) return pathDiagnostics;
        if (Program.Diagnostics.HasErrors()) return Program.Diagnostics;
        return EmitToFiles(moduleName, references, canonicalOutputPath, canonicalSymbolPath, initializedGlobalVariables);
    }
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, Stream outputStream, Stream? symbolStream)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        return Emitter.Emit(Program, moduleName, references, outputStream, symbolStream, ImmutableDictionary<GlobalVariableSymbol, object>.Empty).Diagnostics;
    }
    public EmitterResult Emit(string moduleName, string[] references, Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        return Emitter.Emit(Program, moduleName, references, outputStream, symbolStream, initializedGlobalVariables);
    }
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        return Emitter.Emit(Program, moduleName, references, outputStream, symbolStream, ImmutableDictionary<GlobalVariableSymbol, object>.Empty).Diagnostics;
    }
    public EmitterResult EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        return Emitter.Emit(Program, moduleName, references, outputStream, symbolStream, initializedGlobalVariables);
    }

    ImmutableArray<Diagnostic> EmitToFiles(
        string moduleName,
        EmitReferenceSet references,
        string outputPath,
        string? symbolPath,
        ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        var referenceDiagnostics = references.GetDiagnostics();
        if (referenceDiagnostics.HasErrors()) return referenceDiagnostics;

        string? temporaryOutputPath = null;
        string? temporarySymbolPath = null;
        string? symbolBackupPath = null;
        try
        {
            EmitterResult result;
            using (var outputStream = CreateTemporaryFile(outputPath, out temporaryOutputPath))
            {
                if (symbolPath is null)
                    result = EmitWithReferenceSet(moduleName, references, outputStream, null, initializedGlobalVariables);
                else
                    using (var symbolStream = CreateTemporaryFile(symbolPath, out temporarySymbolPath))
                        result = Emitter.Emit(Program, moduleName, references, outputStream, symbolStream, initializedGlobalVariables, symbolPath);
            }

            if (result.Diagnostics.HasErrors()) return result.Diagnostics;

            if (symbolPath is not null)
            {
                if (File.Exists(symbolPath)) symbolBackupPath = CreateTemporaryPath(symbolPath, "bak");
                CommitTemporaryFile(temporarySymbolPath!, symbolPath, symbolBackupPath);
                temporarySymbolPath = null;
            }

            try
            {
                CommitTemporaryFile(temporaryOutputPath, outputPath);
            }
            catch
            {
                if (symbolPath is not null)
                {
                    var backupPath = symbolBackupPath;
                    symbolBackupPath = null;
                    RestoreFile(symbolPath, backupPath);
                }
                throw;
            }

            temporaryOutputPath = null;
            return result.Diagnostics;
        }
        finally
        {
            DeleteTemporaryFile(temporaryOutputPath);
            DeleteTemporaryFile(temporarySymbolPath);
            DeleteTemporaryFile(symbolBackupPath);
        }
    }

    ImmutableArray<Diagnostic> ValidateEmitPaths(string outputPath, string? symbolPath, out string canonicalOutputPath, out string? canonicalSymbolPath)
    {
        canonicalOutputPath = Path.GetFullPath(outputPath);
        canonicalSymbolPath = string.IsNullOrWhiteSpace(symbolPath) ? null : Path.GetFullPath(symbolPath);
        var diagnostics = new DiagnosticBag();

        if (canonicalSymbolPath is not null && pathComparer.Equals(canonicalOutputPath, canonicalSymbolPath))
            diagnostics.ReportEmitPathCollision("assembly output", canonicalOutputPath, "symbol output", canonicalSymbolPath);

        foreach (var sourcePath in SyntaxTrees.Select(tree => tree.Text.FileName).Where(fileName => !string.IsNullOrWhiteSpace(fileName)).Select(Path.GetFullPath))
        {
            if (pathComparer.Equals(canonicalOutputPath, sourcePath))
                diagnostics.ReportEmitPathCollision("assembly output", canonicalOutputPath, "source file", sourcePath);
            if (canonicalSymbolPath is not null && pathComparer.Equals(canonicalSymbolPath, sourcePath))
                diagnostics.ReportEmitPathCollision("symbol output", canonicalSymbolPath, "source file", sourcePath);
        }

        return diagnostics.ToImmutableArray();
    }

    static FileStream CreateTemporaryFile(string destinationPath, out string temporaryPath)
    {
        temporaryPath = CreateTemporaryPath(destinationPath, "tmp");
        return new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    }

    static string CreateTemporaryPath(string destinationPath, string extension) =>
        Path.Combine(Path.GetDirectoryName(destinationPath)!, $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.{extension}");

    static void CommitTemporaryFile(string temporaryPath, string destinationPath, string? backupPath = null)
    {
        if (File.Exists(destinationPath))
            File.Replace(temporaryPath, destinationPath, backupPath);
        else
            File.Move(temporaryPath, destinationPath);
    }

    static void RestoreFile(string destinationPath, string? backupPath)
    {
        if (backupPath is null)
            File.Delete(destinationPath);
        else
            File.Replace(backupPath, destinationPath, null);
    }

    static void DeleteTemporaryFile(string? path)
    {
        if (path is not null && File.Exists(path)) File.Delete(path);
    }

    public void WriteSyntaxTrees(TextWriter writer)
    {
        foreach (var syntaxTree in SyntaxTrees)
            SyntaxTreePrinter.Print(syntaxTree.Root, writer ?? throw new ArgumentNullException(nameof(writer)));
    }
    public void WriteBoundGlobalTree(TextWriter writer) => WriteBoundFunctionTree(writer, Program.EntryPoint);
    public void WriteBoundFunctionTree(TextWriter writer, FunctionSymbol function)
    {
        _ = function ?? throw new ArgumentNullException(nameof(function));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));

        function.WriteTo(writer);
        writer.WriteLine();
        if (!Program.Functions.TryGetValue(function, out var body))
            writer.WritePunctuation("<no body>");
        else
            BoundTreePrinter.Print(body, writer);
        writer.WriteLine();
    }
    public void WriteControlFlowGraph(TextWriter writer, FunctionSymbol function)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var cfg = ControlFlowGraph.Create(Program.Functions[function]);
        cfg.WriteTo(writer);
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees) => new (false, null, syntaxTrees);
    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees) => new(true, previous, syntaxTrees);

}
