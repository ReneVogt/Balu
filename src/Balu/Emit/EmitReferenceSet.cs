using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Mono.Cecil;

namespace Balu.Emit;

public sealed class EmitReferenceSet : IDisposable
{
    static class RequiredMethodParameters
    {
        static readonly string[] SingleObject = ["System.Object"];
        static readonly string[] Empty = [];

        public static readonly string[] ConsoleWrite = SingleObject;
        public static readonly string[] ConsoleWriteLine = SingleObject;
        public static readonly string[] ConsoleReadLine = Empty;
        public static readonly string[] StringConcat2 = ["System.String", "System.String"];
        public static readonly string[] StringConcat3 = ["System.String", "System.String", "System.String"];
        public static readonly string[] StringConcat4 = ["System.String", "System.String", "System.String", "System.String"];
        public static readonly string[] StringConcatArray = ["System.String[]"];
        public static readonly string[] ConvertToBool = SingleObject;
        public static readonly string[] ConvertToInt = SingleObject;
        public static readonly string[] ConvertToString = SingleObject;
        public static readonly string[] ObjectEquals = ["System.Object", "System.Object"];
        public static readonly string[] RandomCtor = Empty;
        public static readonly string[] RandomNext = ["System.Int32"];
        public static readonly string[] DebuggableCtor = ["System.Boolean", "System.Boolean"];
    }

    static readonly string[] requiredTypeNames =
    [
        "System.Object",
        "System.Console",
        "System.String",
        "System.Convert",
        "System.Int32",
        "System.Boolean",
        "System.Void",
        "System.Random",
        "System.Diagnostics.DebuggableAttribute"
    ];

    readonly object gate = new();
    readonly List<AssemblyDefinition> assemblies = [];
    readonly ImmutableArray<Diagnostic> diagnostics;
    ResolvedReferences? resolvedReferences;
    bool isDisposed;

    public EmitReferenceSet(string[] references)
    {
        _ = references ?? throw new ArgumentNullException(nameof(references));
        var referencePaths = references.ToArray();
        var diagnosticBag = new DiagnosticBag();
        try
        {
            LoadReferences(referencePaths, diagnosticBag);
            if (!diagnosticBag.HasErrors())
                resolvedReferences = ResolveReferences(diagnosticBag);

            diagnostics = diagnosticBag.ToImmutableArray();
            if (diagnostics.HasErrors()) DisposeAssemblies();
        }
        catch
        {
            DisposeAssemblies();
            throw;
        }
    }

    internal ReferencedMembers CreateReferencedMembers(string moduleName)
    {
        lock (gate)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(EmitReferenceSet));
            if (diagnostics.HasErrors()) throw new MissingReferencesException(diagnostics);
            return new(moduleName, resolvedReferences!);
        }
    }

    internal ImmutableArray<Diagnostic> GetDiagnostics()
    {
        lock (gate)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(EmitReferenceSet));
            return diagnostics;
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed) return;
            DisposeAssemblies();
            resolvedReferences = null;
            isDisposed = true;
        }
    }

    void LoadReferences(string[] references, DiagnosticBag diagnosticBag)
    {
        foreach (var reference in references)
        {
            try
            {
                assemblies.Add(AssemblyDefinition.ReadAssembly(reference, new ReaderParameters { InMemory = true }));
            }
            catch (BadImageFormatException exception)
            {
                diagnosticBag.ReportInvalidAssemblyReference(reference, exception.Message);
            }
            catch (IOException exception)
            {
                diagnosticBag.ReportInvalidAssemblyReference(reference, exception.Message);
            }
        }
    }

    ResolvedReferences? ResolveReferences(DiagnosticBag diagnosticBag)
    {
        var candidates = requiredTypeNames.ToDictionary(name => name, _ => new List<TypeDefinition>(), StringComparer.Ordinal);
        foreach (var type in assemblies.SelectMany(assembly => assembly.Modules).SelectMany(module => module.Types))
            if (candidates.TryGetValue(type.FullName, out var definitions))
                definitions.Add(type);

        var types = new Dictionary<string, TypeDefinition?>(StringComparer.Ordinal);
        foreach (var name in requiredTypeNames)
        {
            var definitions = candidates[name];
            if (definitions.Count == 1)
                types.Add(name, definitions[0]);
            else
            {
                types.Add(name, null);
                if (definitions.Count == 0)
                    diagnosticBag.ReportRequiredTypeNotFound(name);
                else
                    diagnosticBag.ReportRequiredTypeAmbiguous(name, [.. definitions]);
            }
        }

        if (diagnosticBag.HasErrors()) return null;

        var objectType = types["System.Object"]!;
        var consoleType = types["System.Console"]!;
        var stringType = types["System.String"]!;
        var convertType = types["System.Convert"]!;
        var intType = types["System.Int32"]!;
        var boolType = types["System.Boolean"]!;
        var voidType = types["System.Void"]!;
        var randomType = types["System.Random"]!;
        var debuggableAttributeType = types["System.Diagnostics.DebuggableAttribute"]!;

        var consoleWrite = ResolveMethod(consoleType, "Write", RequiredMethodParameters.ConsoleWrite, diagnosticBag);
        var consoleWriteLine = ResolveMethod(consoleType, "WriteLine", RequiredMethodParameters.ConsoleWriteLine, diagnosticBag);
        var consoleReadLine = ResolveMethod(consoleType, "ReadLine", RequiredMethodParameters.ConsoleReadLine, diagnosticBag);
        var stringConcat2 = ResolveMethod(stringType, "Concat", RequiredMethodParameters.StringConcat2, diagnosticBag);
        var stringConcat3 = ResolveMethod(stringType, "Concat", RequiredMethodParameters.StringConcat3, diagnosticBag);
        var stringConcat4 = ResolveMethod(stringType, "Concat", RequiredMethodParameters.StringConcat4, diagnosticBag);
        var stringConcatArray = ResolveMethod(stringType, "Concat", RequiredMethodParameters.StringConcatArray, diagnosticBag);
        var convertToBool = ResolveMethod(convertType, "ToBoolean", RequiredMethodParameters.ConvertToBool, diagnosticBag);
        var convertToInt = ResolveMethod(convertType, "ToInt32", RequiredMethodParameters.ConvertToInt, diagnosticBag);
        var convertToString = ResolveMethod(convertType, "ToString", RequiredMethodParameters.ConvertToString, diagnosticBag);
        var objectEquals = ResolveMethod(objectType, "Equals", RequiredMethodParameters.ObjectEquals, diagnosticBag);
        var randomCtor = ResolveMethod(randomType, ".ctor", RequiredMethodParameters.RandomCtor, diagnosticBag);
        var randomNext = ResolveMethod(randomType, "Next", RequiredMethodParameters.RandomNext, diagnosticBag);
        var debuggableCtor = ResolveMethod(debuggableAttributeType, ".ctor", RequiredMethodParameters.DebuggableCtor, diagnosticBag);
        if (diagnosticBag.HasErrors()) return null;

        return new(
            objectType,
            stringType,
            intType,
            boolType,
            voidType,
            randomType,
            consoleWrite!,
            consoleWriteLine!,
            consoleReadLine!,
            stringConcat2!,
            stringConcat3!,
            stringConcat4!,
            stringConcatArray!,
            convertToBool!,
            convertToInt!,
            convertToString!,
            objectEquals!,
            randomCtor!,
            randomNext!,
            debuggableCtor!);
    }

    static MethodDefinition? ResolveMethod(TypeDefinition type, string name, string[] parameterTypeNames, DiagnosticBag diagnosticBag)
    {
        var method = type.Methods.FirstOrDefault(candidate =>
            candidate.Name == name && candidate.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypeNames));
        if (method is null)
            diagnosticBag.ReportRequiredMethodNotFound(type.FullName, name, parameterTypeNames);
        return method;
    }

    void DisposeAssemblies()
    {
        foreach (var assembly in assemblies)
            assembly.Dispose();
        assemblies.Clear();
    }

    internal sealed class ResolvedReferences(
        TypeDefinition objectType,
        TypeDefinition stringType,
        TypeDefinition intType,
        TypeDefinition boolType,
        TypeDefinition voidType,
        TypeDefinition randomType,
        MethodDefinition consoleWrite,
        MethodDefinition consoleWriteLine,
        MethodDefinition consoleReadLine,
        MethodDefinition stringConcat2,
        MethodDefinition stringConcat3,
        MethodDefinition stringConcat4,
        MethodDefinition stringConcatArray,
        MethodDefinition convertToBool,
        MethodDefinition convertToInt,
        MethodDefinition convertToString,
        MethodDefinition objectEquals,
        MethodDefinition randomCtor,
        MethodDefinition randomNext,
        MethodDefinition debuggableCtor)
    {
        public TypeDefinition ObjectType { get; } = objectType;
        public TypeDefinition StringType { get; } = stringType;
        public TypeDefinition IntType { get; } = intType;
        public TypeDefinition BoolType { get; } = boolType;
        public TypeDefinition VoidType { get; } = voidType;
        public TypeDefinition RandomType { get; } = randomType;
        public MethodDefinition ConsoleWrite { get; } = consoleWrite;
        public MethodDefinition ConsoleWriteLine { get; } = consoleWriteLine;
        public MethodDefinition ConsoleReadLine { get; } = consoleReadLine;
        public MethodDefinition StringConcat2 { get; } = stringConcat2;
        public MethodDefinition StringConcat3 { get; } = stringConcat3;
        public MethodDefinition StringConcat4 { get; } = stringConcat4;
        public MethodDefinition StringConcatArray { get; } = stringConcatArray;
        public MethodDefinition ConvertToBool { get; } = convertToBool;
        public MethodDefinition ConvertToInt { get; } = convertToInt;
        public MethodDefinition ConvertToString { get; } = convertToString;
        public MethodDefinition ObjectEquals { get; } = objectEquals;
        public MethodDefinition RandomCtor { get; } = randomCtor;
        public MethodDefinition RandomNext { get; } = randomNext;
        public MethodDefinition DebuggableCtor { get; } = debuggableCtor;
    }
}
