﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Balu.Binding;
using Balu.Evaluation;
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
        SyntaxTrees = syntaxTrees.ToImmutableArray();
        IsScript = isScript;
    }

    public EvaluationResult Evaluate(VariableDictionary globals)
    {
        _ = globals ?? throw new ArgumentNullException(nameof(globals));

        var diagnostics = SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics).Concat(Program.Diagnostics).ToImmutableArray();
        if (diagnostics.Any())
            return new(diagnostics, null);

        return new(ImmutableArray<Diagnostic>.Empty, Evaluator.Evaluate(Program, globals));
    }

    public void WriteSyntaxTrees(TextWriter writer)
    {
        foreach (var syntaxTree in SyntaxTrees)
            SyntaxTreePrinter.Print(syntaxTree.Root, writer ?? throw new ArgumentNullException(nameof(writer)));
    }
    public void WriteProgramTree(TextWriter writer) => BoundTreePrinter.Print(Program, writer ?? throw new ArgumentNullException(nameof(writer)));
    public void WriteTree(TextWriter writer, FunctionSymbol function)
    {
        _ = function ?? throw new ArgumentNullException(nameof(function));
        _ = writer ?? throw new ArgumentNullException(nameof(writer));

        function.WriteTo(writer);
        writer.WriteLine();
        BoundBlockStatement? body = null;
        var prg = Program;
        while (prg is not null && body is null)
        {
            prg.Functions.TryGetValue(function, out body);
            prg = prg.Previous;
        }

        if (body is null)
            writer.WritePunctuation("<no body>");
        else
            BoundTreePrinter.Print(body, writer);
        writer.WriteLine();
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees) => new (false, null, syntaxTrees);
    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees) => new(true, previous, syntaxTrees);

}
