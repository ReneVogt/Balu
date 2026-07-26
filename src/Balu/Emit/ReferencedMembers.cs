using System;
using System.Collections.Immutable;
using Balu.Symbols;
using Mono.Cecil;

namespace Balu.Emit;

sealed class ReferencedMembers : IDisposable
{
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

    public ReferencedMembers(string moduleName, EmitReferenceSet.ResolvedReferences references)
    {
        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        Assembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Dll);
        try
        {
            ConsoleWrite = Assembly.MainModule.ImportReference(references.ConsoleWrite);
            ConsoleWriteLine = Assembly.MainModule.ImportReference(references.ConsoleWriteLine);
            ConsoleReadLine = Assembly.MainModule.ImportReference(references.ConsoleReadLine);
            StringConcat2 = Assembly.MainModule.ImportReference(references.StringConcat2);
            StringConcat3 = Assembly.MainModule.ImportReference(references.StringConcat3);
            StringConcat4 = Assembly.MainModule.ImportReference(references.StringConcat4);
            StringConcatArray = Assembly.MainModule.ImportReference(references.StringConcatArray);
            ConvertToBool = Assembly.MainModule.ImportReference(references.ConvertToBool);
            ConvertToInt = Assembly.MainModule.ImportReference(references.ConvertToInt);
            ConvertToString = Assembly.MainModule.ImportReference(references.ConvertToString);
            ObjectEquals = Assembly.MainModule.ImportReference(references.ObjectEquals);
            var randomReference = Assembly.MainModule.ImportReference(references.RandomType);
            RandomCtor = Assembly.MainModule.ImportReference(references.RandomCtor);
            RandomNext = Assembly.MainModule.ImportReference(references.RandomNext);
            DebuggableAttributeCtor = Assembly.MainModule.ImportReference(references.DebuggableCtor);

            var typeMapBuilder = ImmutableDictionary.CreateBuilder<TypeSymbol, TypeReference>();
            typeMapBuilder.Add(TypeSymbol.Void, Assembly.MainModule.ImportReference(references.VoidType));
            typeMapBuilder.Add(TypeSymbol.Any, Assembly.MainModule.ImportReference(references.ObjectType));
            typeMapBuilder.Add(TypeSymbol.Boolean, Assembly.MainModule.ImportReference(references.BoolType));
            typeMapBuilder.Add(TypeSymbol.Integer, Assembly.MainModule.ImportReference(references.IntType));
            typeMapBuilder.Add(TypeSymbol.String, Assembly.MainModule.ImportReference(references.StringType));
            TypeMap = typeMapBuilder.ToImmutable();

            ProgramType = new(string.Empty, "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, TypeMap[TypeSymbol.Any]);
            RandomField = new(GlobalSymbolNames.Random, FieldAttributes.Static | FieldAttributes.SpecialName, randomReference);
            ProgramType.Fields.Add(RandomField);
            Assembly.MainModule.Types.Add(ProgramType);
        }
        catch
        {
            Assembly.Dispose();
            throw;
        }
    }
    public void Dispose() => Assembly.Dispose();

}
