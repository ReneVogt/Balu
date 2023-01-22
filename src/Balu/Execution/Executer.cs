using Balu.Emit;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Balu.Binding;
using Balu.Symbols;
using Balu.Diagnostics;

namespace Balu.Execution;

static class Executer
{
    public static ExecutionResult Execute(BoundProgram program, string[] referencedAssemblies, ImmutableDictionary<Symbol, object> initializedGlobalSymbols, bool ignoreWarnings)
    {
        using var memoryStream = new MemoryStream();
        var emitterResult = Emitter.Emit(program, "BaluInterpreter", referencedAssemblies, memoryStream, null, initializedGlobalSymbols);
        if (emitterResult.Diagnostics.HasErrors() || !ignoreWarnings && emitterResult.Diagnostics.Any())
            return new(emitterResult.Diagnostics, null, initializedGlobalSymbols);

        memoryStream.Seek(0, SeekOrigin.Begin);
        var context = new AssemblyLoadContext(null, true);
        try
        {
            var asm = context.LoadFromStream(memoryStream);
            var result = asm.EntryPoint!.Invoke(null, null);
            var programType = asm.GetType("Program")!;

            var globals = emitterResult.GlobalSymbolNames
                                       .Where(x => x.Key is GlobalVariableSymbol && !x.Key.Name.IsSpecialName())
                                       .ToImmutableDictionary(
                                           x => x.Key,
                                           x => programType.GetField(x.Value, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!);
            return new(emitterResult.Diagnostics, result, globals);
        }
        finally
        {
            context.Unload();
        }
    }
}