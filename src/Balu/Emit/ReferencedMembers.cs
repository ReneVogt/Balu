using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Symbols;
using Mono.Cecil;

namespace Balu.Emit;

sealed class ReferencedMembers : IDisposable
{
    static class ReferencedMethodParameters
    {
        static readonly string[] SingleObject = ["System.Object"];
        static readonly string[] Empty = [];

        public static readonly string[] ConsoleWrite = SingleObject ;
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
    readonly DiagnosticBag diagnostics = [];
    readonly List<AssemblyDefinition> referencedAssemblies = [];

    public  MethodReference DebuggableAttributeCtor { get; }

    public AssemblyDefinition Assembly { get; }
    public TypeDefinition ProgramType { get; }


    public MethodReference ConsoleWrite { get; }
    public MethodReference ConsoleWriteLine { get; }
    public MethodReference ConsoleReadLine { get; }
    public MethodReference StringConcat2 { get; }
    public MethodReference StringConcat3 { get; }
    public MethodReference StringConcat4 { get; }
    public MethodReference StringConcatArray { get; }
    public MethodReference ConvertToBool { get; }
    public MethodReference ConvertToString { get; }
    public MethodReference ConvertToInt { get; }
    public MethodReference ObjectEquals { get; }

    public FieldDefinition RandomField { get; }
    public MethodReference RandomNext { get; }
    public MethodReference RandomCtor { get; }

    public ImmutableDictionary<TypeSymbol, TypeReference> TypeMap { get; }

    public ReferencedMembers(string moduleName, string[] referencedAssemblies)
    {
        LoadReferences(referencedAssemblies);
        if (diagnostics.HasErrors()) throw new MissingReferencesException(diagnostics.ToImmutableArray());

        var objectTypeDefinition = ResolveTypeDefinition("System.Object");
        var consoleTypeDefinition = ResolveTypeDefinition("System.Console");
        var stringTypeDefinition = ResolveTypeDefinition("System.String");
        var convertTypeDefinition = ResolveTypeDefinition("System.Convert");
        var intTypeDefinition = ResolveTypeDefinition("System.Int32");
        var boolTypeDefinition = ResolveTypeDefinition("System.Boolean");
        var voidTypeDefinition = ResolveTypeDefinition("System.Void");
        var randomTypeDefinition = ResolveTypeDefinition("System.Random");
        var debuggableAttributeTypeDefinition = ResolveTypeDefinition("System.Diagnostics.DebuggableAttribute");
        if (diagnostics.HasErrors() ||
            objectTypeDefinition is null ||
            consoleTypeDefinition is null ||
            stringTypeDefinition is null ||
            convertTypeDefinition is null ||
            intTypeDefinition is null ||
            boolTypeDefinition is null ||
            voidTypeDefinition is null ||
            randomTypeDefinition is null ||
            debuggableAttributeTypeDefinition is null) throw new MissingReferencesException(diagnostics.ToImmutableArray());

        var consoleWrite = ResolveMethodDefinition(consoleTypeDefinition, "Write", ReferencedMethodParameters.ConsoleWrite);
        var consoleWriteLine = ResolveMethodDefinition(consoleTypeDefinition, "WriteLine", ReferencedMethodParameters.ConsoleWriteLine);
        var consoleReadLine = ResolveMethodDefinition(consoleTypeDefinition, "ReadLine", ReferencedMethodParameters.ConsoleReadLine);
        var stringConcat2 = ResolveMethodDefinition(stringTypeDefinition, "Concat", ReferencedMethodParameters.StringConcat2);
        var stringConcat3 = ResolveMethodDefinition(stringTypeDefinition, "Concat", ReferencedMethodParameters.StringConcat3);
        var stringConcat4 = ResolveMethodDefinition(stringTypeDefinition, "Concat", ReferencedMethodParameters.StringConcat4);
        var stringConcatArray = ResolveMethodDefinition(stringTypeDefinition, "Concat", ReferencedMethodParameters.StringConcatArray);
        var convertToBool = ResolveMethodDefinition(convertTypeDefinition, "ToBoolean", ReferencedMethodParameters.ConvertToBool);
        var convertToInt = ResolveMethodDefinition(convertTypeDefinition, "ToInt32", ReferencedMethodParameters.ConvertToInt);
        var convertToString = ResolveMethodDefinition(convertTypeDefinition, "ToString", ReferencedMethodParameters.ConvertToString);
        var objectEquals = ResolveMethodDefinition(objectTypeDefinition, "Equals", ReferencedMethodParameters.ObjectEquals);
        var randomCtor = ResolveMethodDefinition(randomTypeDefinition, ".ctor", ReferencedMethodParameters.RandomCtor);
        var randomNext = ResolveMethodDefinition(randomTypeDefinition, "Next", ReferencedMethodParameters.RandomNext);
        var debuggableCtor = ResolveMethodDefinition(debuggableAttributeTypeDefinition, ".ctor", ReferencedMethodParameters.DebuggableCtor);
        if (diagnostics.HasErrors()) throw new MissingReferencesException(diagnostics.ToImmutableArray());
        Debug.Assert(
            consoleWrite is not null &&
            consoleWriteLine is not null &&
            consoleReadLine is not null &&
            stringConcat2 is not null &&
            stringConcat3 is not null &&
            stringConcat4 is not null &&
            stringConcatArray is not null &&
            convertToBool is not null &&
            convertToInt is not null &&
            convertToString is not null &&
            objectEquals is not null &&
            randomCtor is not null &&
            randomNext is not null && 
            debuggableCtor is not null);

        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        Assembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Dll);

        ConsoleWrite = Assembly.MainModule.ImportReference(consoleWrite);
        ConsoleWriteLine = Assembly.MainModule.ImportReference(consoleWriteLine);
        ConsoleReadLine = Assembly.MainModule.ImportReference(consoleReadLine);
        StringConcat2 = Assembly.MainModule.ImportReference(stringConcat2);
        StringConcat3 = Assembly.MainModule.ImportReference(stringConcat3);
        StringConcat4 = Assembly.MainModule.ImportReference(stringConcat4);
        StringConcatArray = Assembly.MainModule.ImportReference(stringConcatArray);
        ConvertToBool = Assembly.MainModule.ImportReference(convertToBool);
        ConvertToInt = Assembly.MainModule.ImportReference(convertToInt);
        ConvertToString = Assembly.MainModule.ImportReference(convertToString);
        ObjectEquals = Assembly.MainModule.ImportReference(objectEquals);
        var randomReference = Assembly.MainModule.ImportReference(randomTypeDefinition);
        RandomCtor = Assembly.MainModule.ImportReference(randomCtor);
        RandomNext = Assembly.MainModule.ImportReference(randomNext);
        DebuggableAttributeCtor = Assembly.MainModule.ImportReference(debuggableCtor);

        var typeMapBuilder = ImmutableDictionary.CreateBuilder<TypeSymbol, TypeReference>();
        typeMapBuilder.Add(TypeSymbol.Void, Assembly.MainModule.ImportReference(voidTypeDefinition));
        typeMapBuilder.Add(TypeSymbol.Any, Assembly.MainModule.ImportReference(objectTypeDefinition));
        typeMapBuilder.Add(TypeSymbol.Boolean, Assembly.MainModule.ImportReference(boolTypeDefinition));
        typeMapBuilder.Add(TypeSymbol.Integer, Assembly.MainModule.ImportReference(intTypeDefinition));
        typeMapBuilder.Add(TypeSymbol.String, Assembly.MainModule.ImportReference(stringTypeDefinition));
        TypeMap = typeMapBuilder.ToImmutable();

        ProgramType = new(string.Empty, "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, TypeMap[TypeSymbol.Any]);
        RandomField = new (GlobalSymbolNames.Random, FieldAttributes.Static | FieldAttributes.SpecialName, randomReference);
        ProgramType.Fields.Add(RandomField);
        Assembly.MainModule.Types.Add(ProgramType);
    }
    public void Dispose() => Assembly.Dispose();

    void LoadReferences(string[] references)
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
    TypeDefinition? ResolveTypeDefinition(string fullName, TypeSymbol? typeSybmol = null)
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
    MethodDefinition? ResolveMethodDefinition(TypeDefinition typeDefinition, string name, string[] parameterTypeNames)
    {
        var candidates = from method in typeDefinition.Methods
                         where method.Name == name &&
                               method.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(parameterTypeNames)
                         select method;
        var winner = candidates.FirstOrDefault();
        if (winner is not null) return winner;
        diagnostics.ReportRequiredMethodNotFound(typeDefinition.FullName, name, parameterTypeNames);
        return null;
    }

}
