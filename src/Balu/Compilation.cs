using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Balu.Binding;
using Balu.Emit;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Visualization;
using Binder = Balu.Binding.Binder;

#pragma warning disable CA1724
namespace Balu;

public sealed class Compilation
{
    BoundGlobalScope? globalScope;
    BoundProgram? program;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (globalScope is null)
            {
                var scope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
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
                var p = Binder.BindProgram(IsScript, Previous?.Program, GlobalScope);
                Interlocked.CompareExchange(ref program, p, null);
            }

            return program;
        }
    }

    public bool IsScript { get; }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public Compilation? Previous { get; }

    public FunctionSymbol? MainFunction => GlobalScope.EntryPoint;
    public ImmutableArray<Symbol> Symbols => GlobalScope.Symbols;
    public IEnumerable<Symbol> AllVisibleSymbols
    {
        get
        {
            var uniqueNames = new HashSet<string>();
            var submission = this;
            while (submission is not null)
            {
                foreach (var symbol in submission.Symbols.Where(symbol => uniqueNames.Add(symbol.Name)))
                        yield return symbol;
                submission = submission.Previous;
            }

            var builtIniFunctions = from property in typeof(BuiltInFunctions).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                    where property.PropertyType == typeof(FunctionSymbol)
                                    select (FunctionSymbol)property.GetValue(null)!;
            foreach (var builtInFunction in builtIniFunctions.Where(symbol => uniqueNames.Add(symbol.Name)))
                yield return builtInFunction;
        }
    }

    Compilation(bool isScript, Compilation? previous, params SyntaxTree[] syntaxTrees)
    {
        Previous = previous;
        SyntaxTrees = syntaxTrees.DefaultIfEmpty(SyntaxTree.Parse(string.Empty)).ToImmutableArray();
        IsScript = isScript;
    }

    public EvaluationResult Evaluate(ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        using var memoryStream = new MemoryStream();
        var emitterResult = Emitter.Emit(Program, "BaluInterpreter", ReferencedAssembliesFinder.GetReferences(), memoryStream, null, initializedGlobalVariables);
        if (emitterResult.Diagnostics.Any())
            return new(emitterResult.Diagnostics, null, initializedGlobalVariables);

        var rawAssembly = memoryStream.GetBuffer();
        var asm = Assembly.Load(rawAssembly);
        var result = asm.EntryPoint!.Invoke(null, null);
        var programType = asm.GetType("Program")!;

        var globals = emitterResult.GlobalFieldNames
                                   .Where(x => !x.Key.Name.IsSpecialName())
                                   .ToImmutableDictionary(
                                       x => x.Key, x => programType.GetField(x.Value, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!);

        return new(ImmutableArray<Diagnostic>.Empty, result, globals);
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
        if (!Program.AllVisibleFunctions.TryGetValue(function, out var body))
            writer.WritePunctuation("<no body>");
        else
            BoundTreePrinter.Print(body, writer);
        writer.WriteLine();
    }
    public void WriteControlFlowGraph(TextWriter writer, FunctionSymbol function)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var cfg = ControlFlowGraph.Create(Program.AllVisibleFunctions[function]);
        cfg.WriteTo(writer);
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees) => new (false, null, syntaxTrees);
    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees) => new(true, previous, syntaxTrees);

}
