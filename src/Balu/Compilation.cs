using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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

    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath, string? symbolPath)
    {
        _ = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        _ = references ?? throw new ArgumentNullException(nameof(references));
        _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

        Stream? symbolStream = string.IsNullOrWhiteSpace(symbolPath) ? null : File.Create(symbolPath);
        try
        {
            using var outputStream = File.Create(outputPath);
            return Emit(moduleName, references, outputStream, symbolStream);
        }
        finally
        {
            symbolStream?.Dispose();
        }
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
