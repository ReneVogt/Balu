using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Balu.Binding;
using Balu.Diagnostics;
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
    readonly DiagnosticBag diagnostics = new();

    readonly ReferencedMembers referencedMembers;

    readonly Dictionary<FunctionSymbol, MethodDefinition> methods = new();
    readonly Dictionary<LocalVariableSymbol, VariableDefinition> locals = new();
    readonly Dictionary<GlobalVariableSymbol, FieldDefinition> globals = new();
    readonly Dictionary<BoundLabel, int> labels = new();
    readonly List<(int instrcutionIndex, BoundLabel label)> gotosToFix = new();
    readonly Dictionary<SourceText, Document> documents = new();
    readonly Dictionary<Symbol, string> globalSymbolNames = new();
    readonly BoundLabel exitLabel = new("<exit>");

    bool debug;

    Emitter(BoundProgram program, string moduleName, string[] references)
    {
        this.program = program;

        diagnostics.AddRange(program.Diagnostics);
        referencedMembers = new(moduleName, references);
    }
    public void Dispose() => referencedMembers.Dispose();

    TypeReference MapType(TypeSymbol typeSymbol) => referencedMembers.TypeMap.TryGetValue(typeSymbol, out var reference) ? reference : throw new EmitterException($"Invalid type symbol '{typeSymbol}'");

    MethodDefinition CreateMethod(FunctionSymbol function)
    {
        bool isVisilbe = function == program.EntryPoint || program.VisibleSymbols.Contains(function);
        var name = isVisilbe ? function.Name : $"<{function.Name}{globals.Count}>";
        var attributes = MethodAttributes.Static | MethodAttributes.Private;
        if (name.IsBaluSpecialName()) attributes |= MethodAttributes.SpecialName;
        var method = new MethodDefinition(name, attributes, MapType(function.ReturnType));
        foreach (var parameter in function.Parameters)
            method.Parameters.Add(new (parameter.Name, ParameterAttributes.None, MapType(parameter.Type)));
        
        referencedMembers.ProgramType.Methods.Add(method);
        globalSymbolNames[function] = name;
        return method;
    }
    void EmitMethod(MethodDefinition method, FunctionSymbol function)
    {
        var processor = method.Body.GetILProcessor();

        locals.Clear();
        gotosToFix.Clear();
        labels.Clear();

        var body = program.Functions[function];

        if (debug)
        {
            if (function.Declaration is { Body.OpenBraceToken: var openBrace})
                EmitSequencePointStatement(processor, new(openBrace, new BoundNopStatement(openBrace), openBrace.Location));
            else if (body.Syntax.SyntaxTree.Text.Length > 0)
            {
                var firstToken = body.Syntax.SyntaxTree.Root.FirstToken;
                EmitSequencePointStatement(
                    processor, new(firstToken, new BoundNopStatement(firstToken), new(firstToken.SyntaxTree.Text, new(0, 1))));
            }
        }

        foreach (var statement in body.Statements)
            EmitStatement(processor, statement);

        if (debug)
        {
            labels.Add(exitLabel, processor.Body.Instructions.Count);
            var index = processor.Body.Instructions.Count;
            processor.Emit(OpCodes.Ret);
            var instruction = processor.Body.Instructions[index];
            if (function.Declaration is { Body.ClosedBraceToken.Location : var closedBraceLocation })
                AddSequencePoint(processor, instruction, closedBraceLocation);
            else if (body.Syntax.SyntaxTree.Text.Length > 0)
            {
                var endLocation = body.Syntax.LastToken.Location;
                var start = Math.Max(0, Math.Min(endLocation.Span.End, endLocation.Text.Length - 1));
                var exitLocation = endLocation with { Span = new(start, 1) };
                AddSequencePoint(processor, instruction, exitLocation);
            }
        }

        foreach ((int instrcutionIndex, BoundLabel? label) in gotosToFix)
            processor.Body.Instructions[instrcutionIndex].Operand = processor.Body.Instructions[labels[label]];
        
        method.Body.OptimizeMacros();

        if (method.Body.Instructions.Any())
        {
            method.DebugInformation.Scope = new(method.Body.Instructions.First(), method.Body.Instructions.Last());
            foreach (var x in locals)
                method.DebugInformation.Scope.Variables.Add(new(x.Value, x.Key.Name));
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
            case BoundNodeKind.PrefixExpression:
                EmitPrefixExpression(processor, (BoundPrefixExpression)expression);
                break;
            case BoundNodeKind.PostfixExpression:
                EmitPostfixExpression(processor, (BoundPostfixExpression)expression);
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
        if (IsStringConcat(expression))
        {
            EmitStringConcat(processor, expression);
            return;
        }

        if (expression.Operator.OperatorKind is BoundBinaryOperatorKind.LogicalAnd or BoundBinaryOperatorKind.LogicalOr)
        {
            EmitLogicalBinaryExpression(processor, expression);
            return;
        }

        EmitExpression(processor, expression.Left);
        EmitExpression(processor, expression.Right);

        switch (expression.Operator.OperatorKind)
        {
            case BoundBinaryOperatorKind.Addition:
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
    static bool IsStringConcat(BoundExpression expression) =>
        expression is BoundBinaryExpression { Operator.OperatorKind: BoundBinaryOperatorKind.Addition, Type: var type } && type == TypeSymbol.String;
    void EmitStringConcat(ILProcessor processor, BoundBinaryExpression expression)
    {
        var builder = ImmutableArray.CreateBuilder<BoundExpression>();
        StringBuilder? constantBuilder = null;
        var stack = new Stack<BoundBinaryExpression>();
        var current = expression;
        while (IsStringConcat(current.Left))
        {
            stack.Push(current);
            current = (BoundBinaryExpression)current.Left;
        }

        AddOperand(current.Left);
        AddOperand(current.Right);
        while (stack.Count > 0)
        {
            current = stack.Pop();
            AddOperand(current.Right);
        }

        void AddOperand(BoundExpression operand)
        {
            if (operand is { Constant.Value: string constant, HasSideEffects: false })
            {
                constantBuilder ??= new();
                constantBuilder.Append(constant);
            }
            else
            {
                if (constantBuilder?.Length > 0)
                {
                    builder.Add(new BoundLiteralExpression(expression.Syntax, constantBuilder.ToString()));
                    constantBuilder.Clear();
                }

                builder.Add(operand);
            }
        }
        if (constantBuilder?.Length > 0)
            builder.Add(new BoundLiteralExpression(expression.Syntax, constantBuilder.ToString()));

        var operands = builder.ToImmutable();
        switch (operands.Length)
        {
            case 0:
                processor.Emit(OpCodes.Ldstr, string.Empty);
                break;
            case 1:
                EmitExpression(processor, operands[0]);
                break;
            case 2:
                EmitExpression(processor, operands[0]);
                EmitExpression(processor, operands[1]);
                processor.Emit(OpCodes.Call, referencedMembers.StringConcat2);
                break;
            case 3:
                EmitExpression(processor, operands[0]);
                EmitExpression(processor, operands[1]);
                EmitExpression(processor, operands[2]);
                processor.Emit(OpCodes.Call, referencedMembers.StringConcat3);
                break;
            case 4:
                EmitExpression(processor, operands[0]);
                EmitExpression(processor, operands[1]);
                EmitExpression(processor, operands[2]);
                EmitExpression(processor, operands[3]);
                processor.Emit(OpCodes.Call, referencedMembers.StringConcat4);
                break;
            default:
                processor.Emit(OpCodes.Ldc_I4, operands.Length);
                processor.Emit(OpCodes.Newarr, MapType(TypeSymbol.String));

                for (var i = 0; i < operands.Length; i++)
                {
                    processor.Emit(OpCodes.Dup);
                    processor.Emit(OpCodes.Ldc_I4, i);
                    EmitExpression(processor, operands[i]);
                    processor.Emit(OpCodes.Stelem_Ref);
                }

                processor.Emit(OpCodes.Call, referencedMembers.StringConcatArray);
                break;

        }
    }
    void EmitLogicalBinaryExpression(ILProcessor processor, BoundBinaryExpression expression)
    {
        EmitExpression(processor, expression.Left);
        processor.Emit(OpCodes.Dup);
        var label = new BoundLabel("logical");
        gotosToFix.Add((processor.Body.Instructions.Count, label));
        processor.Emit(expression.Operator.OperatorKind == BoundBinaryOperatorKind.LogicalAnd ? OpCodes.Brfalse : OpCodes.Brtrue,
                           Instruction.Create(OpCodes.Nop));
        processor.Emit(OpCodes.Pop);
        EmitExpression(processor, expression.Right);
        labels.Add(label, processor.Body.Instructions.Count);

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
    void EmitVariableExpression(ILProcessor processor, BoundVariableExpression expression) => EmitLoadVarialble(processor, expression.Variable);
    void EmitLoadVarialble(ILProcessor processor, VariableSymbol variable)
    {
        switch (variable.Kind)
        {
            case SymbolKind.Parameter:
                processor.Emit(OpCodes.Ldarg, ((ParameterSymbol)variable).Ordinal);
                break;
            case SymbolKind.LocalVariable:
                var variableDefinition = locals[(LocalVariableSymbol)variable];
                processor.Emit(OpCodes.Ldloc, variableDefinition);
                break;
            case SymbolKind.GlobalVariable:
                var fieldDefinition = globals[(GlobalVariableSymbol)variable];
                processor.Emit(OpCodes.Ldsfld, fieldDefinition);
                break;
            default:
                throw new EmitterException($"Unexpected symbol kind '{variable.Kind}' when loading a variable.");
        }

    }
    void EmitWriteVariable(ILProcessor processor, VariableSymbol variable)
    {
        switch (variable.Kind)
        {
            case SymbolKind.Parameter:
                processor.Emit(OpCodes.Starg, ((ParameterSymbol)variable).Ordinal);
                break;
            case SymbolKind.GlobalVariable:
                processor.Emit(OpCodes.Stsfld, globals[(GlobalVariableSymbol)variable]);
                break;
            case SymbolKind.LocalVariable:
                processor.Emit(OpCodes.Stloc, locals[(LocalVariableSymbol)variable]);
                break;
            default:
                throw new EmitterException($"Unexpected symbol kind '{variable.Kind}' when storing a variable..");
        }

    }
    void EmitAssignmentExpression(ILProcessor processor, BoundAssignmentExpression expression)
    {
        EmitExpression(processor, expression.Expression);
        processor.Emit(OpCodes.Dup);
        EmitWriteVariable(processor, expression.Symbol);
    }
    void EmitCallExpression(ILProcessor processor, BoundCallExpression expression)
    {
        if (expression.Function == BuiltInFunctions.Random)
            processor.Emit(OpCodes.Ldsfld, referencedMembers.RandomField);

        foreach (var argument in expression.Arguments)
            EmitExpression(processor, argument);

        if (expression.Function == BuiltInFunctions.Print)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleWrite);
        else if (expression.Function == BuiltInFunctions.PrintLine)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleWriteLine);
        else if (expression.Function == BuiltInFunctions.Input)
            processor.Emit(OpCodes.Call, referencedMembers.ConsoleReadLine);
        else if (expression.Function == BuiltInFunctions.Random)
            processor.Emit(OpCodes.Callvirt, referencedMembers.RandomNext);
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
    void EmitPrefixExpression(ILProcessor processor, BoundPrefixExpression expression)
    {
        EmitLoadVarialble(processor, expression.Variable);
        processor.Emit(OpCodes.Ldc_I4_1);
        processor.Emit(expression.Operator.OperatorKind == BoundBinaryOperatorKind.Addition ? OpCodes.Add : OpCodes.Sub);
        processor.Emit(OpCodes.Dup);
        EmitWriteVariable(processor, expression.Variable);
    }
    void EmitPostfixExpression(ILProcessor processor, BoundPostfixExpression expression)
    {
        EmitLoadVarialble(processor, expression.Variable);
        processor.Emit(OpCodes.Dup);
        processor.Emit(OpCodes.Ldc_I4_1);
        processor.Emit(expression.Operator.OperatorKind == BoundBinaryOperatorKind.Addition ? OpCodes.Add : OpCodes.Sub);
        EmitWriteVariable(processor, expression.Variable);
    }
    void EmitVariableDeclarationStatement(ILProcessor processor, BoundVariableDeclarationStatement statement)
    {
        EmitExpression(processor, statement.Expression);
        switch (statement.Variable.Kind)
        {
            case SymbolKind.LocalVariable:
                var variableDefinition = new VariableDefinition(MapType(statement.Variable.Type));
                locals.Add((LocalVariableSymbol)statement.Variable, variableDefinition);
                processor.Body.Variables.Add(variableDefinition);
                processor.Emit(OpCodes.Stloc, variableDefinition);
                break;
            case SymbolKind.GlobalVariable:
                // global variables were already added as fields 
                var global = (GlobalVariableSymbol)statement.Variable;
                processor.Emit(OpCodes.Stsfld, globals[global]);
                break;
            default:
                throw new EmitterException($"Unexpected symbol kind '{statement.Variable.Kind}' in variable declaration statement.");

        }
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
        if (!debug)
            processor.Emit(OpCodes.Ret);
        else
        {
            gotosToFix.Add((processor.Body.Instructions.Count, exitLabel));
            processor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }
    }
    static void EmitNopStatement(ILProcessor processor)
    {
        processor.Emit(OpCodes.Nop);
    }
    void EmitSequencePointStatement(ILProcessor processor, BoundSequencePointStatement statement)
    {
        if (!debug)
        {
            if (statement.Statement.Kind != BoundNodeKind.NopStatement)
                EmitStatement(processor, statement.Statement);
            return;
        }

        var index = processor.Body.Instructions.Count;
        EmitStatement(processor, statement.Statement);
        var instruction = processor.Body.Instructions[index];
        AddSequencePoint(processor, instruction, statement.Location);
    }
    void AddSequencePoint(ILProcessor processor, Instruction instruction, TextLocation location)
    {
        if (!documents.TryGetValue(location.Text, out var document))
        {
            var uriString = Uri.TryCreate(location.FileName, UriKind.RelativeOrAbsolute, out var uri) ? uri.ToString() : string.Empty;
            document = new(uriString);
            documents[location.Text] = document;
        }

        var sequencePoint = new SequencePoint(instruction, document)
        {
            StartLine = location.StartLine + 1,
            StartColumn = location.StartCharacter + 1,
            EndLine = location.EndLine + 1,
            EndColumn = location.EndCharacter + 1
        };
        processor.Body.Method.DebugInformation.SequencePoints.Add(sequencePoint);

    }

    void EmitFields(ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        var staticConstructor = new MethodDefinition(
            ".cctor", 
            MethodAttributes.Static | 
            MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | 
            MethodAttributes.Private,
            MapType(TypeSymbol.Void));
        referencedMembers.ProgramType.Methods.Add(staticConstructor);
        var processor = staticConstructor.Body.GetILProcessor();

        processor.Emit(OpCodes.Newobj, referencedMembers.RandomCtor);
        processor.Emit(OpCodes.Stsfld, referencedMembers.RandomField);

        foreach(var global in program.Symbols.OfType<GlobalVariableSymbol>())
        {
            EmitField(global);
            if (!initializedGlobalVariables.TryGetValue(global, out var value)) continue;
            bool box = true;
            switch (value)
            {
                case false:
                    processor.Emit(OpCodes.Ldc_I4_0);
                    break;
                case true:
                    processor.Emit(OpCodes.Ldc_I4_1);
                    break;
                case int i:
                    processor.Emit(OpCodes.Ldc_I4, i);
                    break;
                case string s:
                    processor.Emit(OpCodes.Ldstr, s);
                    box = false;
                    break;
                default:
                    throw new EmitterException($"Invalid value type '{value.GetType().Name}' in global variables.");
            }

            if (box && global.Type == TypeSymbol.Any)
                processor.Emit(OpCodes.Box);
            processor.Emit(OpCodes.Stsfld, globals[global]);
        }

        processor.Emit(OpCodes.Ret);
    }
    void EmitField(GlobalVariableSymbol global)
    {
        bool isVisible = program.VisibleSymbols.Contains(global);
        var name = isVisible ? global.Name : $"<{global.Name}{globals.Count}>";
        var attributes = FieldAttributes.Static | FieldAttributes.Private;
        if (!isVisible) attributes |= FieldAttributes.SpecialName;
        var fieldDefinition = new FieldDefinition(name, attributes, MapType(global.Type));
        referencedMembers.ProgramType.Fields.Add(fieldDefinition);
        globals.Add(global, fieldDefinition);
        globalSymbolNames[global] = name;
    }

    void EmitDebuggableAttribute()
    {
        if (!debug) return;
        var attribute = new CustomAttribute(referencedMembers.DebuggableAttributeCtor);
        attribute.ConstructorArguments.Add(new (MapType(TypeSymbol.Boolean), true));
        attribute.ConstructorArguments.Add(new (MapType(TypeSymbol.Boolean), true));
        referencedMembers.Assembly.CustomAttributes.Add(attribute);
        
        attribute = new(referencedMembers.DebuggableAttributeCtor);
        attribute.ConstructorArguments.Add(new(MapType(TypeSymbol.Boolean), true));
        attribute.ConstructorArguments.Add(new(MapType(TypeSymbol.Boolean), true));
        referencedMembers.Assembly.MainModule.CustomAttributes.Add(attribute);
    }

    void Emit(Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        if (diagnostics.HasErrors()) return;

        debug = symbolStream is not null;
        EmitDebuggableAttribute();

        EmitFields(initializedGlobalVariables);

        foreach (var function in program.Functions.Keys)
            methods.Add(function, CreateMethod(function));

        EmitMethod(methods[program.EntryPoint], program.EntryPoint);
        foreach (var x in methods.Where(x => x.Key != program.EntryPoint))
            EmitMethod(x.Value, x.Key);

        referencedMembers.Assembly.EntryPoint = methods[program.EntryPoint];

        if (symbolStream is null)
            referencedMembers.Assembly.Write(outputStream);
        else
        {
            var writerParameters = new WriterParameters
            {
                WriteSymbols = true,
                SymbolStream = symbolStream,
                SymbolWriterProvider = new PortablePdbWriterProvider()
            };
            referencedMembers.Assembly.Write(outputStream, writerParameters);
        }
    }

    public static EmitterResult Emit(BoundProgram program, string moduleName, string[] references, Stream outputStream, Stream? symbolStream, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobalVariables)
    {
        try
        {
            if (program.Diagnostics.HasErrors()) return new(program.Diagnostics, ImmutableDictionary<Symbol, string>.Empty);
            using var emitter = new Emitter(program, moduleName, references);
            emitter.Emit(outputStream, symbolStream, initializedGlobalVariables);
            return new(emitter.diagnostics.ToImmutableArray(), emitter.globalSymbolNames.ToImmutableDictionary());
        }
        catch (MissingReferencesException exception)
        {
            return new (exception.Diagnostics, ImmutableDictionary<Symbol, string>.Empty);
        }
    }
}