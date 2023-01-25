using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using Balu.Visualization;

namespace Balu.Interpretation;

public sealed class Interpreter
{
    public Compilation Compilation { get; private set; } = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
    public object? Result { get; private set; }
    public ImmutableArray<Symbol> VisibleSymbols => Compilation.VisibleSymbols;
    public ImmutableArray<Symbol> AllSymbols => Compilation.AllSymbols;
    public ImmutableDictionary<GlobalVariableSymbol, object> GlobalVariables { get; private set; } = ImmutableDictionary<GlobalVariableSymbol, object>.Empty;

    public TextWriter? Out { get; set; }
    public TextWriter? Error { get; set; }
    public bool WriteSyntax { get; set; }
    public bool WriteProgram { get; set; }

    public ImmutableArray<Diagnostic> Emit(string path, string? symbolPath = null) => Compilation.Emit(
        "BaluInterpreter", ReferencedAssembliesFinder.GetReferences(), path ?? throw new ArgumentNullException(nameof(path)), symbolPath, GlobalVariables);
    public void Reset()
    {
        Compilation = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
    }
    public ImmutableArray<Diagnostic> Execute(string code, bool ignoreWarnings = true)
    {
        var compilation = Compilation.CreateScript(Compilation, SyntaxTree.Parse(SourceText.From(code, "BaluInterpreter.b")));
        var referencedAssemblies = ReferencedAssembliesFinder.GetReferences();

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
        var emitterResult = compilation.Emit("BaluInterpreter", referencedAssemblies, memoryStream, null, GlobalVariables);
        Error?.WriteDiagnostics(emitterResult.Diagnostics);

        if (emitterResult.Diagnostics.HasErrors() || !ignoreWarnings && emitterResult.Diagnostics.Any())
            return emitterResult.Diagnostics;

        Compilation = compilation;

        memoryStream.Seek(0, SeekOrigin.Begin);
        var context = new AssemblyLoadContext(null, true);
        try
        {
            var asm = context.LoadFromStream(memoryStream);
            Result = asm.EntryPoint!.Invoke(null, null);
            var programType = asm.GetType("Program")!;

            GlobalVariables = emitterResult.GlobalSymbolNames
                                       .Where(x => x.Key is GlobalVariableSymbol && !x.Key.Name.IsBaluSpecialName())
                                       .ToImmutableDictionary(
                                           x => (GlobalVariableSymbol)x.Key,
                                           x => programType.GetField(x.Value, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!);
            return emitterResult.Diagnostics;
        }
        finally
        {
            context.Unload();
        }
    }

}