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
    readonly CancellationToken cancellationToken;
    BoundGlobalScope? globalScope;
    BoundProgram? program;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            return GetGlobalScope(cancellationToken);
        }
    }
    internal BoundProgram Program
    {
        get
        {
            return GetProgram(cancellationToken);
        }
    }

    public bool IsScript { get; }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public ImmutableArray<Diagnostic> Diagnostics => GetProgram(cancellationToken).Diagnostics;

    public FunctionSymbol? MainFunction => GlobalScope.EntryPoint;
    public ImmutableArray<Symbol> VisibleSymbols => GlobalScope.VisibleSymbols;
    public ImmutableArray<Symbol> AllSymbols => GlobalScope.AllSymbols;

    Compilation(bool isScript, Compilation? previous, CancellationToken cancellationToken, params SyntaxTree[] syntaxTrees)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (previous?.GetProgram(cancellationToken).Diagnostics.HasErrors() == true)
            throw new ArgumentException("A compilation can only be continued if it does not contain any errors.", nameof(previous));
        this.previous = previous;
        this.cancellationToken = cancellationToken;
        SyntaxTrees = syntaxTrees.DefaultIfEmpty(SyntaxTree.Parse(string.Empty, cancellationToken)).ToImmutableArray();
        IsScript = isScript;
    }

    BoundGlobalScope GetGlobalScope(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (globalScope is null)
        {
            var scope = Binder.BindGlobalScope(IsScript, previous?.GetGlobalScope(cancellationToken), SyntaxTrees, cancellationToken);
            Interlocked.CompareExchange(ref globalScope, scope, null);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return globalScope;
    }

    BoundProgram GetProgram(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (program is null)
        {
            var p = Binder.BindProgram(IsScript, previous?.GetProgram(cancellationToken), GetGlobalScope(cancellationToken), cancellationToken);
            Interlocked.CompareExchange(ref program, p, null);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return program;
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath) =>
        Emit(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty);
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath, CancellationToken cancellationToken) =>
        Emit(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken);
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath, bool debug, CancellationToken cancellationToken = default) =>
        Emit(moduleName, references, outputPath, symbolPath, debug, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken);
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
        => Emit(moduleName, references, outputPath, symbolPath, symbolPath is not null, initializedGlobalVariables, cancellationToken);
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath, bool debug, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        cancellationToken.ThrowIfCancellationRequested();

        var pathDiagnostics = ValidateEmitPaths(outputPath, symbolPath, out var canonicalOutputPath, out var canonicalSymbolPath, cancellationToken);
        if (pathDiagnostics.HasErrors()) return pathDiagnostics;
        var program = GetProgram(cancellationToken);
        if (program.Diagnostics.HasErrors()) return program.Diagnostics;

        using var referenceSet = new EmitReferenceSet(references);
        return EmitToFiles(moduleName, referenceSet, canonicalOutputPath, canonicalSymbolPath, debug, initializedGlobalVariables, cancellationToken);
    }
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath) =>
        EmitWithReferenceSet(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty);
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath, CancellationToken cancellationToken) =>
        EmitWithReferenceSet(moduleName, references, outputPath, symbolPath, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken);
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath, bool debug, CancellationToken cancellationToken = default) =>
        EmitWithReferenceSet(moduleName, references, outputPath, symbolPath, debug, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken);
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
        => EmitWithReferenceSet(moduleName, references, outputPath, symbolPath, symbolPath is not null, initializedGlobalVariables, cancellationToken);
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, string outputPath, string? symbolPath, bool debug, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        cancellationToken.ThrowIfCancellationRequested();

        var pathDiagnostics = ValidateEmitPaths(outputPath, symbolPath, out var canonicalOutputPath, out var canonicalSymbolPath, cancellationToken);
        if (pathDiagnostics.HasErrors()) return pathDiagnostics;
        var program = GetProgram(cancellationToken);
        if (program.Diagnostics.HasErrors()) return program.Diagnostics;
        return EmitToFiles(moduleName, references, canonicalOutputPath, canonicalSymbolPath, debug, initializedGlobalVariables, cancellationToken);
    }
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, Stream outputStream, Stream? symbolStream, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken: cancellationToken).Diagnostics;
    }
    public EmitterResult Emit(string moduleName, string[] references, Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, initializedGlobalVariables, cancellationToken);
    }
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken: cancellationToken).Diagnostics;
    }
    public ImmutableArray<Diagnostic> EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream, bool debug, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, debug, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken: cancellationToken).Diagnostics;
    }
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, Stream outputStream, Stream? symbolStream, bool debug, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        using var referenceSet = new EmitReferenceSet(references);
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, referenceSet, outputStream, symbolStream, debug, ImmutableDictionary<GlobalVariableSymbol, object>.Empty, cancellationToken: cancellationToken).Diagnostics;
    }
    public EmitterResult EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
        => EmitWithReferenceSet(moduleName, references, outputStream, symbolStream, symbolStream is not null, initializedGlobalVariables, cancellationToken);
    public EmitterResult EmitWithReferenceSet(string moduleName, EmitReferenceSet references, Stream outputStream, Stream? symbolStream, bool debug, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables, CancellationToken cancellationToken = default)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        Emitter.ValidateEmitStreams(outputStream, symbolStream);
        cancellationToken.ThrowIfCancellationRequested();
        return Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, debug, initializedGlobalVariables, cancellationToken: cancellationToken);
    }

    ImmutableArray<Diagnostic> EmitToFiles(
        string moduleName,
        EmitReferenceSet references,
        string outputPath,
        string? symbolPath,
        bool debug,
        ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
                cancellationToken.ThrowIfCancellationRequested();
                if (symbolPath is null)
                    result = EmitWithReferenceSet(moduleName, references, outputStream, null, initializedGlobalVariables, cancellationToken);
                else
                    using (var symbolStream = CreateTemporaryFile(symbolPath, out temporarySymbolPath))
                        result = Emitter.Emit(GetProgram(cancellationToken), moduleName, references, outputStream, symbolStream, debug, initializedGlobalVariables, symbolPath, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (result.Diagnostics.HasErrors()) return result.Diagnostics;

            if (symbolPath is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(symbolPath)) symbolBackupPath = CreateTemporaryPath(symbolPath, "bak");
                CommitTemporaryFile(temporarySymbolPath!, symbolPath, symbolBackupPath);
                temporarySymbolPath = null;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
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

    ImmutableArray<Diagnostic> ValidateEmitPaths(string outputPath, string? symbolPath, out string canonicalOutputPath, out string? canonicalSymbolPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        canonicalOutputPath = Path.GetFullPath(outputPath);
        canonicalSymbolPath = string.IsNullOrWhiteSpace(symbolPath) ? null : Path.GetFullPath(symbolPath);
        var diagnostics = new DiagnosticBag();

        if (canonicalSymbolPath is not null && pathComparer.Equals(canonicalOutputPath, canonicalSymbolPath))
            diagnostics.ReportEmitPathCollision("assembly output", canonicalOutputPath, "symbol output", canonicalSymbolPath);

        foreach (var sourcePath in SyntaxTrees.Select(tree => tree.Text.FileName).Where(fileName => !string.IsNullOrWhiteSpace(fileName)).Select(Path.GetFullPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
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

    public void WriteSyntaxTrees(TextWriter writer, CancellationToken cancellationToken = default)
    {
        foreach (var syntaxTree in SyntaxTrees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SyntaxTreePrinter.Print(syntaxTree.Root, writer ?? throw new ArgumentNullException(nameof(writer)));
        }
    }
    public void WriteBoundGlobalTree(TextWriter writer, CancellationToken cancellationToken = default) => WriteBoundFunctionTree(writer, GetProgram(cancellationToken).EntryPoint, cancellationToken);
    public void WriteBoundFunctionTree(TextWriter writer, FunctionSymbol function, CancellationToken cancellationToken = default)
    {
        _ = function ?? throw new ArgumentNullException(nameof(function));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        cancellationToken.ThrowIfCancellationRequested();

        function.WriteTo(writer);
        writer.WriteLine();
        if (!GetProgram(cancellationToken).Functions.TryGetValue(function, out var body))
            writer.WritePunctuation("<no body>");
        else
            BoundTreePrinter.Print(body, writer);
        writer.WriteLine();
    }
    public void WriteControlFlowGraph(TextWriter writer, FunctionSymbol function, CancellationToken cancellationToken = default)
    {
        _ = function ?? throw new ArgumentNullException(nameof(function));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        cancellationToken.ThrowIfCancellationRequested();
        if (!GetProgram(cancellationToken).Functions.TryGetValue(function, out var body))
            throw new ArgumentException($"Function '{function.Name}' does not have a body in this compilation.", nameof(function));
        var cfg = ControlFlowGraph.Create(body, cancellationToken);
        cfg.WriteTo(writer);
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees) => new (false, null, default, syntaxTrees);
    public static Compilation Create(CancellationToken cancellationToken, params SyntaxTree[] syntaxTrees) => new(false, null, cancellationToken, syntaxTrees);
    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees) => new(true, previous, default, syntaxTrees);
    public static Compilation CreateScript(Compilation? previous, CancellationToken cancellationToken, params SyntaxTree[] syntaxTrees) => new(true, previous, cancellationToken, syntaxTrees);

}
