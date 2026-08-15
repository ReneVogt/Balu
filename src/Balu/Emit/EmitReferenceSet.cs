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
    readonly record struct RequiredMethod(
        string Name,
        string ReturnTypeName,
        bool IsStatic,
        bool IsConstructor,
        string[] ParameterTypeNames);

    static class RequiredMethods
    {
        static readonly string[] SingleObject = ["System.Object"];
        static readonly string[] Empty = [];

        public static readonly RequiredMethod ConsoleWrite = new("Write", "System.Void", true, false, SingleObject);
        public static readonly RequiredMethod ConsoleWriteLine = new("WriteLine", "System.Void", true, false, SingleObject);
        public static readonly RequiredMethod ConsoleReadLine = new("ReadLine", "System.String", true, false, Empty);
        public static readonly RequiredMethod StringConcat2 = new("Concat", "System.String", true, false, ["System.String", "System.String"]);
        public static readonly RequiredMethod StringConcat3 = new("Concat", "System.String", true, false, ["System.String", "System.String", "System.String"]);
        public static readonly RequiredMethod StringConcat4 = new("Concat", "System.String", true, false, ["System.String", "System.String", "System.String", "System.String"]);
        public static readonly RequiredMethod StringConcatArray = new("Concat", "System.String", true, false, ["System.String[]"]);
        public static readonly RequiredMethod ConvertToBool = new("ToBoolean", "System.Boolean", true, false, SingleObject);
        public static readonly RequiredMethod ConvertToInt = new("ToInt32", "System.Int32", true, false, SingleObject);
        public static readonly RequiredMethod ConvertToString = new("ToString", "System.String", true, false, SingleObject);
        public static readonly RequiredMethod ObjectEquals = new("Equals", "System.Boolean", true, false, ["System.Object", "System.Object"]);
        public static readonly RequiredMethod RandomCtor = new(".ctor", "System.Void", false, true, Empty);
        public static readonly RequiredMethod RandomNext = new("Next", "System.Int32", false, false, ["System.Int32"]);
        public static readonly RequiredMethod DebuggableCtor = new(".ctor", "System.Void", false, true, ["System.Boolean", "System.Boolean"]);
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
            catch (Exception exception) when (exception is ArgumentException or BadImageFormatException or IOException or UnauthorizedAccessException)
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

        var consoleWrite = ResolveMethod(consoleType, RequiredMethods.ConsoleWrite, diagnosticBag);
        var consoleWriteLine = ResolveMethod(consoleType, RequiredMethods.ConsoleWriteLine, diagnosticBag);
        var consoleReadLine = ResolveMethod(consoleType, RequiredMethods.ConsoleReadLine, diagnosticBag);
        var stringConcat2 = ResolveMethod(stringType, RequiredMethods.StringConcat2, diagnosticBag);
        var stringConcat3 = ResolveMethod(stringType, RequiredMethods.StringConcat3, diagnosticBag);
        var stringConcat4 = ResolveMethod(stringType, RequiredMethods.StringConcat4, diagnosticBag);
        var stringConcatArray = ResolveMethod(stringType, RequiredMethods.StringConcatArray, diagnosticBag);
        var convertToBool = ResolveMethod(convertType, RequiredMethods.ConvertToBool, diagnosticBag);
        var convertToInt = ResolveMethod(convertType, RequiredMethods.ConvertToInt, diagnosticBag);
        var convertToString = ResolveMethod(convertType, RequiredMethods.ConvertToString, diagnosticBag);
        var objectEquals = ResolveMethod(objectType, RequiredMethods.ObjectEquals, diagnosticBag);
        var randomCtor = ResolveMethod(randomType, RequiredMethods.RandomCtor, diagnosticBag);
        var randomNext = ResolveMethod(randomType, RequiredMethods.RandomNext, diagnosticBag);
        var debuggableCtor = ResolveMethod(debuggableAttributeType, RequiredMethods.DebuggableCtor, diagnosticBag);
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

    static MethodDefinition? ResolveMethod(TypeDefinition type, RequiredMethod requiredMethod, DiagnosticBag diagnosticBag)
    {
        var methods = type.Methods.Where(candidate =>
            candidate.Name == requiredMethod.Name &&
            candidate.ReturnType.FullName == requiredMethod.ReturnTypeName &&
            candidate.IsPublic &&
            candidate.IsStatic == requiredMethod.IsStatic &&
            candidate.HasThis == !requiredMethod.IsStatic &&
            !candidate.ExplicitThis &&
            candidate.IsConstructor == requiredMethod.IsConstructor &&
            !candidate.HasGenericParameters &&
            candidate.CallingConvention == MethodCallingConvention.Default &&
            candidate.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(requiredMethod.ParameterTypeNames)).ToArray();
        if (methods.Length == 0)
            diagnosticBag.ReportRequiredMethodNotFound(type.FullName, requiredMethod.Name, requiredMethod.ParameterTypeNames);
        else if (methods.Length > 1)
            diagnosticBag.ReportRequiredMethodAmbiguous(type.FullName, requiredMethod.Name, requiredMethod.ParameterTypeNames);
        return methods.Length == 1 ? methods[0] : null;
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
