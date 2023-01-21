using Balu.Emit;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Balu.Binding;
using Balu.Symbols;

namespace Balu.Execution;

static class Executer
{
    public static ExecutionResult Execute(BoundProgram program, string[] referencedAssemblies, ImmutableDictionary<Symbol, object> initializedGlobalSymbols)
    {
        using var memoryStream = new MemoryStream();
        var emitterResult = Emitter.Emit(program, "BaluInterpreter", referencedAssemblies, memoryStream, null, initializedGlobalSymbols);
        if (emitterResult.Diagnostics.Any())
            return new(emitterResult.Diagnostics, null, initializedGlobalSymbols);

        var rawAssembly = memoryStream.GetBuffer();
        var asm = Assembly.Load(rawAssembly);
        var result = asm.EntryPoint!.Invoke(null, null);
        var programType = asm.GetType("Program")!;

        var globals = emitterResult.GlobalSymbolNames
                                   .Where(x => x.Key is GlobalVariableSymbol && !x.Key.Name.IsSpecialName())
                                   .ToImmutableDictionary(
                                       x => x.Key, x => programType.GetField(x.Value, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!);

        return new(ImmutableArray<Diagnostic>.Empty, result, globals);
    }
}