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

sealed class Emitter : IDisposable
{
    readonly BoundProgram program;
    readonly string[] references;
    readonly string outputPath;

    readonly AssemblyDefinition assembly;
    readonly List<AssemblyDefinition> referencedAssemblies = new();

    readonly DiagnosticBag diagnostics = new();
    Emitter(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        this.program = program;
        this.references = references;
        this.outputPath = outputPath;

        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        assembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Dll);
    }
    public void Dispose() => assembly.Dispose();

    void LoadReferences()
    {
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
    }

    TypeDefinition ? ResolveTypeDefinition(string fullName, TypeSymbol? typeSybmol = null)
    {
        var typeDefinitions = referencedAssemblies.SelectMany(referencedAssembly => referencedAssembly.Modules)
                                                  .SelectMany(module => module.Types)
                                                  .Where(t => t.FullName == fullName)
                                                  .ToArray();

        if (typeDefinitions.Length == 1)
            return typeDefinitions[0];

        if (typeDefinitions.Length == 0)
            diagnostics.ReportRequiredTypeNotFound(fullName, typeSybmol);
        else
            diagnostics.ReportRequiredTypeAmbiguous(fullName, typeDefinitions);

        return null;
    }

    MethodReference? ResolveMethod(TypeDefinition typeDefinition, string name, string[] parameterTypeNames)
    {
        var candidates = from method in typeDefinition.Methods
                         where method.Name == name &&
                               method.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(parameterTypeNames)
                         select method;
        var winner = candidates.FirstOrDefault();
        if (winner is not null) return assembly.MainModule.ImportReference(winner);
        
        diagnostics.ReportRequiredMethodNotFound(typeDefinition.FullName, name, parameterTypeNames);
        return null;
    }

    void Emit()
    {
        diagnostics.AddRange(program.Diagnostics);
        if (diagnostics.Any()) return;
        LoadReferences();
        if (diagnostics.Any()) return;

        var voidTypeDefinition = ResolveTypeDefinition("System.Void", TypeSymbol.Void);
        var objectTypeDefinition = ResolveTypeDefinition("System.Object", TypeSymbol.Any);
        var consoleTypeDefinition = ResolveTypeDefinition("System.Console");
        if (objectTypeDefinition is null || voidTypeDefinition is null || consoleTypeDefinition is null) return;
        var objectType = assembly.MainModule.ImportReference(objectTypeDefinition);
        var voidType = assembly.MainModule.ImportReference(voidTypeDefinition);
        var consoleWrite = ResolveMethod(consoleTypeDefinition, "Write", new[] { "System.String" });
        if (consoleWrite is null) return;

        var programType = new TypeDefinition(string.Empty, "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
        assembly.MainModule.Types.Add(programType);
        var main = new MethodDefinition("main", MethodAttributes.Static | MethodAttributes.Private, voidType);
        programType.Methods.Add(main);
        assembly.EntryPoint = main;

        var processor = main.Body.GetILProcessor();

        processor.Emit(OpCodes.Ldstr, "Hello World!");
        processor.Emit(OpCodes.Call, consoleWrite);
        processor.Emit(OpCodes.Ret);

        assembly.Write(outputPath);
    }
    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        using var emitter = new Emitter(program, moduleName, references, outputPath);
        emitter.Emit();
        return emitter.diagnostics.ToImmutableArray();
    }
}