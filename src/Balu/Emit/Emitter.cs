using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Balu.Emit;

static class Emitter
{
    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        if (program.Diagnostics.Length > 0) return program.Diagnostics;
        var diagnostics = new DiagnosticBag();

        List<AssemblyDefinition> referencedAssemblies = new();
        foreach (var reference in references)
        {
            try
            {
                referencedAssemblies.Add(AssemblyDefinition.ReadAssembly(reference));
            }
            catch (BadImageFormatException exception)
            {
                diagnostics.ReportInvalidAssemblyReference(reference, exception.Message);
            }
            catch (IOException exception)
            {
                diagnostics.ReportInvalidAssemblyReference(reference, exception.Message);
            }
        }

        if (diagnostics.Any()) return diagnostics.ToImmutableArray();

        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        using var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);

        var builtInTypes = new Dictionary<TypeSymbol, string>
        {
            { TypeSymbol.Any, "System.Object" },
            { TypeSymbol.Boolean, "System.Boolean" },
            { TypeSymbol.String, "System.String" },
            { TypeSymbol.Integer, "System.Int32" },
            { TypeSymbol.Void, "System.Void" }
        };
        var knownTypes = new Dictionary<TypeSymbol, TypeReference>();
        foreach (var (type, metadataName) in builtInTypes)
        {
            var typeDefinitions = referencedAssemblies.SelectMany(assembly => assembly.Modules)
                                                     .SelectMany(module => module.Types)
                                                     .Where(t => t.FullName == metadataName)
                                                     .ToArray();

            if (typeDefinitions.Length == 0)
                diagnostics.ReportMissingBuiltInType(type);
            else if (typeDefinitions.Length > 1)
                diagnostics.ReportBuiltInTypeAmbiguous(type, typeDefinitions);
            else
                knownTypes[type] = assemblyDefinition.MainModule.ImportReference(typeDefinitions[0]);
        }

        if (diagnostics.Any()) return diagnostics.ToImmutableArray();
        
        var programType = new TypeDefinition(string.Empty, "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, knownTypes[TypeSymbol.Any]);
        assemblyDefinition.MainModule.Types.Add(programType);
        var main = new MethodDefinition("main", MethodAttributes.Static | MethodAttributes.Private, knownTypes[TypeSymbol.Void]);
        programType.Methods.Add(main);
        assemblyDefinition.EntryPoint = main;

        var processor = main.Body.GetILProcessor();
        processor.Emit(OpCodes.Ret);

        assemblyDefinition.Write(outputPath);

        return diagnostics.ToImmutableArray();
    }
}