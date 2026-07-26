using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Balu.Diagnostics;
using Balu.Emit;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using Balu.Visualization;

namespace Balu.Interpretation;

public sealed class Interpreter : IDisposable
{
    int submissionCount;
    readonly EmitReferenceSet references;

    public Interpreter(string[] referencedAssemblies)
    {
        ArgumentNullException.ThrowIfNull(referencedAssemblies);
        references = new(referencedAssemblies);
    }

    public Compilation Compilation { get; private set; } = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
    public object? Result { get; private set; }
    public ImmutableArray<Symbol> VisibleSymbols => Compilation.VisibleSymbols;
    public ImmutableArray<Symbol> AllSymbols => Compilation.AllSymbols;
    public ImmutableDictionary<GlobalVariableSymbol, object> GlobalVariables { get; private set; } = [];

    public TextWriter? Out { get; set; }
    public TextWriter? Error { get; set; }
    public bool WriteSyntax { get; set; }
    public bool WriteProgram { get; set; }

    public ImmutableArray<Diagnostic> Emit(string path, string? symbolPath = null) => Compilation.EmitWithReferenceSet(
        "BaluInterpreter", references, path ?? throw new ArgumentNullException(nameof(path)), symbolPath, GlobalVariables);
    public void Dispose() => references.Dispose();
    public void Reset()
    {
        submissionCount = 0;
        Compilation = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
        Result = null;
        GlobalVariables = [];
    }
    public ImmutableArray<Diagnostic> Execute(string code, bool ignoreWarnings = true)
    {
        var submissionNumber = submissionCount + 1;
        var documentName = $"BaluInterpreter/submission-{submissionNumber:0000}.b";
        var compilation = Compilation.CreateScript(Compilation, SyntaxTree.Parse(SourceText.From(code, documentName)));

        if (Out is not null)
        {
            if (WriteSyntax)
            {
                Out.WriteColoredText("Syntax:", ConsoleColor.Yellow);
                Out.WriteLine();
                compilation.WriteSyntaxTrees(Console.Out);
            }

            if (WriteProgram)
            {
                Out.WriteColoredText("Program:", ConsoleColor.Yellow);
                Out.WriteLine();
                compilation.WriteBoundGlobalTree(Console.Out);
            }
        }

        using var memoryStream = new MemoryStream();
        var emitterResult = compilation.EmitWithReferenceSet("BaluInterpreter", references, memoryStream, null, GlobalVariables);
        Error?.WriteDiagnostics(emitterResult.Diagnostics);

        if (emitterResult.Diagnostics.HasErrors() || !ignoreWarnings && emitterResult.Diagnostics.Any())
            return emitterResult.Diagnostics;

        memoryStream.Seek(0, SeekOrigin.Begin);
        var context = new AssemblyLoadContext(null, true);
        object? result;
        ImmutableDictionary<GlobalVariableSymbol, object> globalVariables;
        try
        {
            var asm = context.LoadFromStream(memoryStream);
            result = asm.EntryPoint!.Invoke(null, null);
            var programType = asm.GetType("Program")!;

            globalVariables = emitterResult.GlobalSymbolNames
                                           .Where(x => x.Key is GlobalVariableSymbol && !x.Key.Name.IsBaluSpecialName())
                                           .ToImmutableDictionary(
                                               x => (GlobalVariableSymbol)x.Key,
                                               x => programType.GetField(x.Value, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!);
        }
        finally
        {
            context.Unload();
        }

        submissionCount = submissionNumber;
        Compilation = compilation;
        Result = result;
        GlobalVariables = globalVariables;
        return emitterResult.Diagnostics;
    }

}
