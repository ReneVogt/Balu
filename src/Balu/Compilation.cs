using System;
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

    public static Compilation Empty { get; } = new Compilation();

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (globalScope is null)
            {
                var scope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
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
                var p = Binder.BindProgram(Previous?.Program, GlobalScope);
                Interlocked.CompareExchange(ref program, p, null);
            }

            return program;
        }
    }

    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public Compilation? Previous { get; }

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

    public Compilation(params SyntaxTree[] syntaxTrees) : this(null, syntaxTrees){}

    Compilation(Compilation? previous, params SyntaxTree[] syntaxTrees) => (Previous, SyntaxTrees) = (previous, syntaxTrees.ToImmutableArray());
    public Compilation ContinueWith(SyntaxTree syntaxTree) => new (this, syntaxTree);


    public EvaluationResult Evaluate(VariableDictionary globals)
    {
        _ = globals ?? throw new ArgumentNullException(nameof(globals));

        var diagnostics = SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics).Concat(GlobalScope.Diagnostics).ToImmutableArray();
        if (diagnostics.Any())
            return new(diagnostics, null);
        diagnostics = Program.Diagnostics;
        if (diagnostics.Any())
            return new(diagnostics, null);

        return new(ImmutableArray<Diagnostic>.Empty, Evaluator.Evaluate(Program, globals));
    }

    public void WriteSyntaxTrees(TextWriter writer)
    {
        foreach (var syntaxTree in SyntaxTrees)
            SyntaxTreeWriter.Print(syntaxTree.Root, writer ?? throw new ArgumentNullException(nameof(writer)));
    }
    public void WriteProgramTree(TextWriter writer) => BoundTreeWriter.Print(Program, writer ?? throw new ArgumentNullException(nameof(writer)));
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
            BoundTreeWriter.Print(body, writer);
        writer.WriteLine();
    }
    public void WriteControlFlowGraph(TextWriter writer)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        var cfg = ControlFlowGraph.Create(!Program.GlobalScope.Statement.Statements.Any() && Program.Functions.Any()
                                              ? Program.Functions.Last().Value
                                              : Program.GlobalScope.Statement);
        cfg.WriteTo(writer);
    }
    public static EvaluationResult Evaluate(string input, VariableDictionary globals) => Evaluate(SyntaxTree.Parse(input ?? throw new ArgumentNullException(nameof(input))), globals);

    public static EvaluationResult Evaluate(SyntaxTree syntaxTree, VariableDictionary globals) => new Compilation(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))).Evaluate(globals);
}
