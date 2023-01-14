using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
namespace Balu.Emit;

sealed class Emitter : IDisposable
{
    readonly BoundProgram program;
    readonly string outputPath;
    readonly DiagnosticBag diagnostics = new();

    readonly ReferencedMembers referencedMembers;

    readonly Dictionary<FunctionSymbol, MethodDefinition> methods = new();
    readonly Dictionary<VariableSymbol, VariableDefinition> locals = new();
    readonly Dictionary<BoundLabel, int> labels = new();
    readonly List<(int instrcutionIndex, BoundLabel label)> gotosToFix = new();
    readonly Dictionary<SourceText, Document> documents = new();

    FieldDefinition? randomField;

    Emitter(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        this.program = program;
        this.outputPath = outputPath;

        diagnostics.AddRange(program.Diagnostics);
        referencedMembers = new(moduleName, references);

        foreach (var function in program.Functions.Keys)
            methods.Add(function, CreateMethod(function));
    }
    public void Dispose() => referencedMembers.Dispose();

    TypeReference MapType(TypeSymbol typeSymbol) => referencedMembers.TypeMap.TryGetValue(typeSymbol, out var reference) ? reference : throw new EmitterException($"Invalid type symbol '{typeSymbol}'");

    MethodDefinition CreateMethod(FunctionSymbol function)
    {
        var method = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, MapType(function.ReturnType));
        foreach (var parameter in function.Parameters)
            method.Parameters.Add(new (parameter.Name, ParameterAttributes.None, MapType(parameter.Type)));
        
        referencedMembers.ProgramType.Methods.Add(method);
        return method;
    }
    void EmitMethod(MethodDefinition method, FunctionSymbol function)
    {
        var processor = method.Body.GetILProcessor();
        
        locals.Clear();
        gotosToFix.Clear();
        labels.Clear();

        var body = program.Functions[function];
        foreach (var statement in body.Statements)
            EmitStatement(processor, statement);

        foreach ((int instrcutionIndex, BoundLabel? label) in gotosToFix)
            processor.Body.Instructions[instrcutionIndex].Operand = processor.Body.Instructions[labels[label]];
        
        method.Body.OptimizeMacros();

        if (method.Body.Instructions.Any())
        {
            method.DebugInformation.Scope = new(method.Body.Instructions.First(), method.Body.Instructions.Last());
            foreach (var (symbol, definition) in locals)
                method.DebugInformation.Scope.Variables.Add(new(definition, symbol.Name));
        }
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
            case BoundNodeKind.NopStatement:
                EmitNopStatement(processor);
                break;
            case BoundNodeKind.SequencePointStatement:
                EmitSequencePointStatement(processor, (BoundSequencePointStatement)statement);
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
        if (expression is { Constant: {}, HasSideEffects: false })
        {
            EmitConstantExpression(processor, expression);
            return;
        }

        switch (expression.Kind)
        {
            case BoundNodeKind.UnaryExpression:
                EmitUnaryExpression(processor, (BoundUnaryExpression)expression);
                break;
            case BoundNodeKind.BinaryExpression:
                EmitBinaryExpression(processor, (BoundBinaryExpression)expression);
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
    void EmitUnaryExpression(ILProcessor processor, BoundUnaryExpression expression)
    {
        EmitExpression(processor, expression.Operand);
        switch (expression.Operator.OperatorKind)
        {
            case BoundUnaryOperatorKind.Identity:
                return;
            case BoundUnaryOperatorKind.Negation:
                processor.Emit(OpCodes.Neg);
                break;
            case BoundUnaryOperatorKind.LogicalNegation:
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            case BoundUnaryOperatorKind.BitwiseNegation:
                processor.Emit(OpCodes.Not);
                break;
            default:
                throw new EmitterException($"Unexpected unary operator kind '{expression.Operator.SyntaxKind.GetText() ?? expression.Operator.OperatorKind.ToString()}'.");
        }
    }
    void EmitBinaryExpression(ILProcessor processor, BoundBinaryExpression expression)
    {
        // TODO: implement short-circuit evaluation for logical operations
        EmitExpression(processor, expression.Left);
        EmitExpression(processor, expression.Right);

        switch (expression.Operator.OperatorKind)
        {
            case BoundBinaryOperatorKind.Addition:
                if (expression.Type == TypeSymbol.String)
                    processor.Emit(OpCodes.Call, referencedMembers.StringConcat);
                else
                    processor.Emit(OpCodes.Add);
                break;
            case BoundBinaryOperatorKind.Substraction:
                processor.Emit(OpCodes.Sub);
                break;
            case BoundBinaryOperatorKind.Multiplication:
                processor.Emit(OpCodes.Mul);
                break;
            case BoundBinaryOperatorKind.Division:
                processor.Emit(OpCodes.Div);
                break;
            case BoundBinaryOperatorKind.LogicalOr:
            case BoundBinaryOperatorKind.BitwiseOr:
                processor.Emit(OpCodes.Or);
                break;
            case BoundBinaryOperatorKind.LogicalAnd:
            case BoundBinaryOperatorKind.BitwiseAnd:
                processor.Emit(OpCodes.And); 
                break;
            case BoundBinaryOperatorKind.BitwiseXor:
                processor.Emit(OpCodes.Xor);
                break;
            case BoundBinaryOperatorKind.Equals:
                if (expression.Left.Type.IsReferenceType)
                    processor.Emit(OpCodes.Call, referencedMembers.ObjectEquals);
                else
                    processor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.NotEqual:
                if (expression.Left.Type.IsReferenceType)
                    processor.Emit(OpCodes.Call, referencedMembers.ObjectEquals);
                else
                    processor.Emit(OpCodes.Ceq);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.Less:
                // compare less than
                processor.Emit(OpCodes.Clt);
                break;
            case BoundBinaryOperatorKind.LessOrEquals:
                // not compare greater than
                processor.Emit(OpCodes.Cgt);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.Greater:
                // compare greater than
                processor.Emit(OpCodes.Cgt);
                break;
            case BoundBinaryOperatorKind.GreaterOrEquals:
                // not compare less than
                processor.Emit(OpCodes.Clt);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            default:
                throw new EmitterException(
                    $"Unexpected binary operator '{expression.Operator.SyntaxKind.GetText() ?? expression.Operator.OperatorKind.ToString()}'.");
        }
    }
    static void EmitConstantExpression(ILProcessor processor, BoundExpression expression)
    {
        if (expression.Type == TypeSymbol.Boolean)
            processor.Emit((bool)expression.Constant!.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        else if (expression.Type == TypeSymbol.Integer)
            processor.Emit(OpCodes.Ldc_I4, (int)expression.Constant!.Value);
        else if (expression.Type == TypeSymbol.String)
            processor.Emit(OpCodes.Ldstr, (string)expression.Constant!.Value);
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
        if (expression.Function == BuiltInFunctions.Random)
        {
            EmitRandomField();
            processor.Emit(OpCodes.Ldsfld, randomField);
        }

        foreach (var argument in expression.Arguments)
            EmitExpression(processor, argument);

        if (expression.Function == BuiltInFunctions.Print)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleWrite);
        else if (expression.Function == BuiltInFunctions.PrintLine)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleWriteLine);
        else if (expression.Function == BuiltInFunctions.Input)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleReadLine);
        else if (expression.Function == BuiltInFunctions.Random)
        {
            EmitRandomField();
            processor.Emit(OpCodes.Callvirt, referencedMembers.RandomNext);
        }
        else
            processor.Emit(OpCodes.Call, methods[expression.Function]);
    }
    void EmitConversionExpression(ILProcessor processor, BoundConversionExpression expression)
    {
        EmitExpression(processor, expression.Expression);
        if (expression.Expression.Type == TypeSymbol.Boolean || expression.Expression.Type == TypeSymbol.Integer)
            processor.Emit(OpCodes.Box, MapType(expression.Expression.Type));

        if (expression.Type == TypeSymbol.Any) return;
        if (expression.Type == TypeSymbol.Boolean)
            processor.Emit(OpCodes.Call, referencedMembers.ConvertToBool);
        else if (expression.Type == TypeSymbol.Integer)
            processor.Emit(OpCodes.Call, referencedMembers.ConvertToInt);
        else if (expression.Type == TypeSymbol.String)
            processor.Emit(OpCodes.Call, referencedMembers.ConvertToString);
        else 
            throw new EmitterException($"Unexpected conversion from '{expression.Expression.Type}' to '{expression.Type}'.");
    }
    void EmitVariableDeclarationStatement(ILProcessor processor, BoundVariableDeclarationStatement statement)
    {
        var variableDefinition = new VariableDefinition(MapType(statement.Variable.Type));
        locals.Add(statement.Variable, variableDefinition);
        processor.Body.Variables.Add(variableDefinition);

        EmitExpression(processor, statement.Expression);
        processor.Emit(OpCodes.Stloc, variableDefinition);
    }
    void EmitGotoStatement(ILProcessor processor, BoundGotoStatement statement)
    {
        gotosToFix.Add((processor.Body.Instructions.Count, statement.Label));
        processor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
    }
    void EmitConditionalGotoStatement(ILProcessor processor, BoundConditionalGotoStatement statement)
    {
        EmitExpression(processor, statement.Condition);  
        gotosToFix.Add((processor.Body.Instructions.Count, statement.Label));
        processor.Emit(statement.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse, Instruction.Create(OpCodes.Nop));
    }
    void EmitLabelStatement(ILProcessor processor, BoundLabelStatement statement)
    {
        labels.Add(statement.Label, processor.Body.Instructions.Count);
    }
    void EmitReturnStatement(ILProcessor processor, BoundReturnStatement statement)
    {
        if (statement.Expression is not null)
            EmitExpression(processor, statement.Expression);
        processor.Emit(OpCodes.Ret);
    }
    static void EmitNopStatement(ILProcessor processor)
    {
        processor.Emit(OpCodes.Nop);
    }
    void EmitSequencePointStatement(ILProcessor processor, BoundSequencePointStatement statement)
    {
        var index = processor.Body.Instructions.Count;
        EmitStatement(processor, statement.Statement);
        var instruction = processor.Body.Instructions[index];

        if (!documents.TryGetValue(statement.Location.Text, out var document))
        {
            var uri = new Uri(statement.Location.FileName).ToString();
            document = new(uri);
            documents[statement.Location.Text] = document;
        }

        var sequencePoint = new SequencePoint(instruction, document)
        {
            StartLine = statement.Location.StartLine + 1,
            StartColumn = statement.Location.StartCharacter + 1,
            EndLine = statement.Location.EndLine + 1,
            EndColumn = statement.Location.EndCharacter + 1
        };
        processor.Body.Method.DebugInformation.SequencePoints.Add(sequencePoint);
    }

    void EmitRandomField()
    {
        if (randomField is not null) return;
        var randomTypeReference = referencedMembers.Assembly.MainModule.ImportReference(referencedMembers.RandomType);
        randomField = new("<random>", FieldAttributes.Static | FieldAttributes.Private | FieldAttributes.SpecialName, randomTypeReference);
        referencedMembers.ProgramType.Fields.Add(randomField);

        var staticConstructor = new MethodDefinition(
            ".cctor", 
            MethodAttributes.Static | 
            MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | 
            MethodAttributes.Private,
            MapType(TypeSymbol.Void));
        referencedMembers.ProgramType.Methods.Add(staticConstructor);
        var processor = staticConstructor.Body.GetILProcessor();
        processor.Emit(OpCodes.Newobj, referencedMembers.RandomCtor);
        processor.Emit(OpCodes.Stsfld, randomField);
        processor.Emit(OpCodes.Ret);
    }

    void EmitDebuggableAttribute(bool debug)
    {
        var attribute = new CustomAttribute(referencedMembers.DebuggableAttributeCtor);
        attribute.ConstructorArguments.Add(new (MapType(TypeSymbol.Boolean), debug));
        attribute.ConstructorArguments.Add(new (MapType(TypeSymbol.Boolean), debug));
        referencedMembers.Assembly.MainModule.CustomAttributes.Add(attribute);
    }

    void Emit(bool debug)
    {
        if (diagnostics.Any()) return;

        EmitDebuggableAttribute(debug);

        foreach (var (function, method) in methods)
            EmitMethod(method, function);

        using var outputStream = File.Create(outputPath);
        using var symbolStream = File.Create(Path.ChangeExtension(outputPath, ".pdb"));
        var writerParameters = new WriterParameters
        {
            WriteSymbols = true,
            SymbolStream = symbolStream,
            SymbolWriterProvider = new PortablePdbWriterProvider()
        };
        referencedMembers.Assembly.EntryPoint = methods[program.EntryPoint];
        referencedMembers.Assembly.Write(outputStream, writerParameters);
    }

    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath, bool debug)
    {
        try
        {
            if (program.Diagnostics.Any()) return program.Diagnostics;
            using var emitter = new Emitter(program, moduleName, references, outputPath);
            emitter.Emit(debug);
            return emitter.diagnostics.ToImmutableArray();
        }
        catch (MissingReferencesException exception)
        {
            return exception.Diagnostics;
        }
    }
}