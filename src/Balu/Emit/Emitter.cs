using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Balu.Emit;

sealed class Emitter : IDisposable
{
    readonly BoundProgram program;
    readonly string[] references;
    readonly string outputPath;
    readonly DiagnosticBag diagnostics = new();

    readonly AssemblyDefinition assembly;
    readonly List<AssemblyDefinition> referencedAssemblies = new();
    readonly TypeDefinition? programType;

    readonly Dictionary<TypeSymbol, TypeReference> typeMap = new();
    readonly Dictionary<FunctionSymbol, MethodDefinition> methods = new();

    readonly Dictionary<VariableSymbol, VariableDefinition> locals = new();

    MethodReference? consoleWrite, consoleReadLine, stringConcat;

    Emitter(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        this.program = program;
        this.references = references;
        this.outputPath = outputPath;

        var assemblyName = new AssemblyNameDefinition(moduleName, new(1, 0));
        assembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Dll);

        diagnostics.AddRange(program.Diagnostics);
        if (diagnostics.Any()) return;
        LoadReferences();
        if (diagnostics.Any()) return;

        ResolveTypes();
        if (diagnostics.Any()) return;
        ResolveMethods();
        if (diagnostics.Any()) return;

        programType = new(string.Empty, "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, MapType(TypeSymbol.Any));
        assembly.MainModule.Types.Add(programType);

        foreach (var function in program.Functions.Keys)
            methods.Add(function, CreateMethod(function));
    }
    public void Dispose() => assembly.Dispose();

    TypeReference MapType(TypeSymbol typeSymbol) => typeMap.TryGetValue(typeSymbol, out var reference) ? reference : throw new EmitterException($"Invalid type symbol '{typeSymbol}'");

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
    void ResolveTypes()
    {
        AddToTypeMap(TypeSymbol.Any, "System.Object");
        AddToTypeMap(TypeSymbol.Boolean, "System.Boolean");
        AddToTypeMap(TypeSymbol.Integer, "System.Int32");
        AddToTypeMap(TypeSymbol.String, "System.String");
        AddToTypeMap(TypeSymbol.Void, "System.Void");

        void AddToTypeMap(TypeSymbol typeSymbol, string fullName)
        {
            var definition = ResolveTypeDefinition(fullName, typeSymbol);
            if (definition is null) return;
            typeMap[typeSymbol] = assembly.MainModule.ImportReference(definition);
        }
    }
    void ResolveMethods()
    {
        var consoleTypeDefinition = ResolveTypeDefinition("System.Console")!;
        var stringTypeDefinition = ResolveTypeDefinition("System.String")!;
        if (diagnostics.Any()) return;

        consoleWrite = ResolveMethod(consoleTypeDefinition, "Write", new[] { "System.String" });
        consoleReadLine = ResolveMethod(consoleTypeDefinition, "ReadLine", Array.Empty<string>());

        stringConcat = ResolveMethod(stringTypeDefinition, "Concat", new[] { "System.String", "System.String" });
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

    MethodDefinition CreateMethod(FunctionSymbol function)
    {
        var method = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, MapType(function.ReturnType));
        foreach (var parameter in function.Parameters)
            method.Parameters.Add(new (parameter.Name, ParameterAttributes.None, MapType(parameter.Type)));
        
        programType!.Methods.Add(method);
        return method;
    }
    void EmitMethod(MethodDefinition method, FunctionSymbol function)
    {
        var processor = method.Body.GetILProcessor();
        
        locals.Clear();

        var body = program.Functions[function];
        foreach (var statement in body.Statements)
            EmitStatement(processor, statement);
        
        method.Body.OptimizeMacros();
    }
    void EmitStatement(ILProcessor processor, BoundStatement statement)
    {
        switch (statement.Kind)
        {
            case BoundNodeKind.ExpressionStatement:
                EmitExpressionStatement(processor, (BoundExpressionStatement)statement);
                break;
            case BoundNodeKind.VariableDeclarationStatement:
                EmitVariableDeclarationStatement(processor, (BoundVariableDeclarationStatement)statement);
                break;
            case BoundNodeKind.GotoStatement:
                EmitGotoStatement(processor, (BoundGotoStatement)statement);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                EmitConditionalGotoStatement(processor, (BoundConditionalGotoStatement)statement);
                break;
            case BoundNodeKind.LabelStatement:
                EmitLabelStatement(processor, (BoundLabelStatement)statement);
                break;
            case BoundNodeKind.ReturnStatement:
                EmitReturnStatement(processor, (BoundReturnStatement)statement);
                break;
            default:
                throw new EmitterException($"Invalid statement kind '{statement.Kind}'.");
        }
    }
    void EmitExpressionStatement(ILProcessor processor, BoundExpressionStatement statement)
    {
        EmitExpression(processor, statement.Expression);
        if (statement.Expression.Type != TypeSymbol.Void)
            processor.Emit(OpCodes.Pop);
    }
    void EmitExpression(ILProcessor processor, BoundExpression expression)
    {
        switch (expression.Kind)
        {
            case BoundNodeKind.UnaryExpression:
                EmitUnaryExpression(processor, (BoundUnaryExpression)expression);
                break;
            case BoundNodeKind.BinaryExpression:
                EmitBinaryExpression(processor, (BoundBinaryExpression)expression);
                break;
            case BoundNodeKind.LiteralExpression:
                EmitLiteralExpression(processor, (BoundLiteralExpression)expression);
                break;
            case BoundNodeKind.VariableExpression:
                EmitVariableExpression(processor, (BoundVariableExpression)expression);
                break;
            case BoundNodeKind.AssignmentExpression:
                EmitAssignmentExpression(processor, (BoundAssignmentExpression)expression);
                break;
            case BoundNodeKind.CallExpression:
                EmitCallExpression(processor, (BoundCallExpression)expression);
                break;
            case BoundNodeKind.ConversionExpression:
                EmitConversionExpression(processor, (BoundConversionExpression)expression);
                break;
            default:
                throw new EmitterException($"Invalid expression kind '{expression.Kind}'.");
        }
    }
    static void EmitUnaryExpression(ILProcessor processor, BoundUnaryExpression expression)
    {
        throw new NotImplementedException();
    }
    void EmitBinaryExpression(ILProcessor processor, BoundBinaryExpression expression)
    {
        if (expression.Left.Type == TypeSymbol.String && expression.Right.Type == TypeSymbol.String &&
            expression.Operator.OperatorKind == BoundBinaryOperatorKind.Addition && expression.Type == TypeSymbol.String)
        {
            EmitExpression(processor, expression.Left);
            EmitExpression(processor, expression.Right);
            processor.Emit(OpCodes.Call, stringConcat);
            return;
        }

        throw new EmitterException(
            $"Invalid binary expression '{expression.Operator.OperatorKind}' '{expression.Left.Type}' x '{expression.Right.Type}' -> '{expression.Type}'.");

    }
    static void EmitLiteralExpression(ILProcessor processor, BoundLiteralExpression expression)
    {
        if (expression.Type == TypeSymbol.Boolean)
            processor.Emit((bool)expression.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        else if (expression.Type == TypeSymbol.Integer)
            processor.Emit(OpCodes.Ldc_I4, (int)expression.Value);
        else if (expression.Type == TypeSymbol.String)
            processor.Emit(OpCodes.Ldstr, (string)expression.Value);
        else
            throw new EmitterException($"Unexpected literal expression type '{expression.Type}'.");
    }
    void EmitVariableExpression(ILProcessor processor, BoundVariableExpression expression)
    {
        if (expression.Variable.Kind == SymbolKind.Parameter)
            processor.Emit(OpCodes.Ldarg, ((ParameterSymbol)expression.Variable).Ordinal);
        else
        {
            var variableDefinition = locals[expression.Variable];
            processor.Emit(OpCodes.Ldloc, variableDefinition);
        }
    }
    void EmitAssignmentExpression(ILProcessor processor, BoundAssignmentExpression expression)
    {
        EmitExpression(processor, expression.Expression);
        processor.Emit(OpCodes.Dup);
        if (expression.Symbol.Kind == SymbolKind.Parameter)
            processor.Emit(OpCodes.Starg, ((ParameterSymbol)expression.Symbol).Ordinal);
        else
            processor.Emit(OpCodes.Stloc, locals[expression.Symbol]);
    }
    void EmitCallExpression(ILProcessor processor, BoundCallExpression expression)
    {
        foreach (var argument in expression.Arguments)
            EmitExpression(processor, argument);

        if (expression.Function == BuiltInFunctions.Print)
            processor.Emit(OpCodes.Call, consoleWrite);
        else if(expression.Function == BuiltInFunctions.Input)
            processor.Emit(OpCodes.Call, consoleReadLine);
        else if (expression.Function == BuiltInFunctions.Random)
            throw new NotImplementedException();
        else
            processor.Emit(OpCodes.Call, methods[expression.Function]);
    }
    static void EmitConversionExpression(ILProcessor processor, BoundConversionExpression expression)
    {
        throw new NotImplementedException();
    }
    void EmitVariableDeclarationStatement(ILProcessor processor, BoundVariableDeclarationStatement statement)
    {
        var variableDefinition = new VariableDefinition(MapType(statement.Variable.Type));
        locals.Add(statement.Variable, variableDefinition);
        processor.Body.Variables.Add(variableDefinition);

        EmitExpression(processor, statement.Expression);
        processor.Emit(OpCodes.Stloc, variableDefinition);
    }
    static void EmitGotoStatement(ILProcessor processor, BoundGotoStatement statement)
    {
        throw new NotImplementedException();
    }
    static void EmitConditionalGotoStatement(ILProcessor processor, BoundConditionalGotoStatement statement)
    {
        throw new NotImplementedException();
    }
    static void EmitLabelStatement(ILProcessor processor, BoundLabelStatement statement)
    {
        throw new NotImplementedException();
    }
    void EmitReturnStatement(ILProcessor processor, BoundReturnStatement statement)
    {
        if (statement.Expression is not null)
            EmitExpression(processor, statement.Expression);
            processor.Emit(OpCodes.Ret);
    }

    void Emit()
    {
        if (diagnostics.Any()) return;

        foreach (var (function, method) in methods)
            EmitMethod(method, function);

        assembly.EntryPoint = methods[program.EntryPoint];
        assembly.Write(outputPath);
    }

    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        using var emitter = new Emitter(program, moduleName, references, outputPath);
        emitter.Emit();
        return emitter.diagnostics.ToImmutableArray();
    }
}