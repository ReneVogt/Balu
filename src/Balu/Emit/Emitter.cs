using System.Collections.Immutable;
using Balu.Binding;
using Mono.Cecil;

namespace Balu.Emit;

static class Emitter
{
    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        if (program.Diagnostics.Length > 0) return program.Diagnostics;

        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        using var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);



        assemblyDefinition.Write(outputPath);
        return ImmutableArray<Diagnostic>.Empty;
    }
}