using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;

namespace Balu.Interpretation;

public sealed class Interpreter
{
    public Compilation Compilation { get; private set; } = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
    public object? Result { get; private set; }
    public ImmutableArray<Symbol> VisibleSymbols => Compilation.VisibleSymbols;
    public ImmutableArray<Symbol> AllSymbols => Compilation.AllSymbols;
    public ImmutableDictionary<GlobalVariableSymbol, object> GlobalVariables { get; private set; } = ImmutableDictionary<GlobalVariableSymbol, object>.Empty;

    public void AddCode(string code)
    {
        _ = code ?? throw new ArgumentNullException(nameof(code));
        Compilation = Compilation.CreateScript(Compilation, SyntaxTree.Parse(SourceText.From(code, "BaluInterpreter.b")));
    }
    public ImmutableArray<Diagnostic> Emit(string path, string? symbolPath = null) => Compilation.Emit(
        "BaluInterpreter", ReferencedAssembliesFinder.GetReferences(), path ?? throw new ArgumentNullException(nameof(path)), symbolPath);
    public void Reset()
    {
        Compilation = Compilation.CreateScript(null, SyntaxTree.Parse(string.Empty));
    }
    public ImmutableArray<Diagnostic> Execute(bool ignoreWarnings = true)
    {
        var referencedAssemblies = ReferencedAssembliesFinder.GetReferences();
        using var memoryStream = new MemoryStream();
        var emitterResult = Compilation.Emit("BaluInterpreter", referencedAssemblies, memoryStream, null, GlobalVariables);

        if (emitterResult.Diagnostics.HasErrors() || !ignoreWarnings && emitterResult.Diagnostics.Any())
            return emitterResult.Diagnostics;

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