using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Balu.Binding;
using Balu.Symbols;
using Balu.Visualization;

namespace Balu.Evaluation;

sealed class Evaluator : BoundTreeVisitor
{
    readonly VariableDictionary globals;
    readonly Stack<VariableDictionary> locals = new();
    readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions;

    public object? Result { get; private set; }

    Evaluator(VariableDictionary globals, ImmutableDictionary<FunctionSymbol,BoundBlockStatement> functions)
    {
        this.globals = globals;
        this.functions = functions;
        locals.Push(new());
    }

    protected override void VisitBoundBlockStatement(BoundBlockStatement blockStatement)
    {
        var labelsToIndices = blockStatement.Statements.Select((statement, index) => (statement, index))
                                            .Where(x => x.statement.Kind == BoundNodeKind.LabelStatement)
                                            .ToDictionary(x => ((BoundLabelStatement)x.statement).Label, x => x.index);

        var index = 0;
        while(index < blockStatement.Statements.Length)
        {
            var current = blockStatement.Statements[index];
            switch (current.Kind)
            {
                case BoundNodeKind.ReturnStatement:
                    var returnStatement = (BoundReturnStatement)current;
                    if (returnStatement.Expression is not null) Visit(returnStatement.Expression);
                    return;
                case BoundNodeKind.GotoStatement:
                    index = labelsToIndices[((BoundGotoStatement)current).Label];
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    var conditionalGoto = (BoundConditionalGotoStatement)current;
                    Visit(conditionalGoto.Condition);
                    if ((bool)Result! == conditionalGoto.JumpIfTrue)
                        index = labelsToIndices[conditionalGoto.Label];
                    else
                        index++;
                    break;
                default:
                    Visit(current);
                    index++;
                    break;
            }
        }
    }

    protected override void VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        Result = literalExpression.Value;
    }
    protected override void VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        Visit(unaryExpression.Operand);
        switch (unaryExpression.Operator.OperatorKind)
        {
            case BoundUnaryOperatorKind.Identity:
                break;
            case BoundUnaryOperatorKind.Negation:
                Result = -(int)Result!;
                break;
            case BoundUnaryOperatorKind.LogicalNegation:
                Result = !(bool)Result!;
                break;
            case BoundUnaryOperatorKind.BitwiseNegation:
                Result = ~(int)Result!;
                break;
            default:
                throw EvaluationException.UnaryOperatorCannotBeEvaluated(unaryExpression.Operator.OperatorKind);
        }
    }
    protected override void VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);
        object left = Result!;
        Visit(binaryExpression.Right);
        object right = Result!;
        Result = binaryExpression.Operator.OperatorKind switch
        {
            BoundBinaryOperatorKind.Addition => binaryExpression.Type == TypeSymbol.String ? (string)left + (string)right : (int)left + (int)right,
            BoundBinaryOperatorKind.Substraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            BoundBinaryOperatorKind.BitwiseAnd => binaryExpression.Type == TypeSymbol.Boolean ? (bool)left && (bool)right : (int)left & (int)right,
            BoundBinaryOperatorKind.BitwiseOr => binaryExpression.Type == TypeSymbol.Boolean ? (bool)left || (bool)right : (int)left | (int)right,
            BoundBinaryOperatorKind.BitwiseXor => binaryExpression.Type == TypeSymbol.Boolean ? (bool)left ^ (bool)right : (int)left ^ (int)right,
            BoundBinaryOperatorKind.Equals => Equals(left, right),
            BoundBinaryOperatorKind.NotEqual => !Equals(left, right),
            BoundBinaryOperatorKind.Less => (int)left < (int)right,
            BoundBinaryOperatorKind.LessOrEquals => (int)left <= (int)right,
            BoundBinaryOperatorKind.Greater => (int)left > (int)right,
            BoundBinaryOperatorKind.GreaterOrEquals => (int)left >= (int)right,
            _ => throw EvaluationException.BinaryOperatorCannotBeEvaluated(binaryExpression.Operator.OperatorKind)
        };
    }
    protected override void VisitBoundVariableExpression(BoundVariableExpression variableExpression)
    {
        Result = variableExpression.Variable.Kind switch
        {
            SymbolKind.GlobalVariable => globals[variableExpression.Variable],
            SymbolKind.LocalVariable or
                SymbolKind.Parameter => locals.Peek()[variableExpression.Variable],
            _ => throw EvaluationException.InvalidSymbolKind(variableExpression.Variable)
        };
    }
    protected override void VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression)
    {
        Visit(assignmentExpression.Expression);
        AssignVariable(assignmentExpression.Symbol, Result);
    }
    protected override void VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        Visit(variableDeclarationStatement.Expression);
        AssignVariable(variableDeclarationStatement.Variable, Result);
    }
    void AssignVariable(VariableSymbol variable, object? value)
    {
        switch (variable.Kind)
        {
            case SymbolKind.GlobalVariable:
                globals[variable] = value;
                break;
            case SymbolKind.LocalVariable:
            case SymbolKind.Parameter:
                locals.Peek()[variable] = value;
                break;
            default:
                throw EvaluationException.InvalidSymbolKind(variable);
        }
    }
    protected override void VisitBoundCallExpression(BoundCallExpression callExpression)
    {
        if (callExpression.Function == BuiltInFunctions.Print)
        {
            ExecutePrint(callExpression.Arguments, false);
            return;
        }
        if (callExpression.Function == BuiltInFunctions.PrintLine)
        {
            ExecutePrint(callExpression.Arguments, true);
            return;
        }

        if (callExpression.Function == BuiltInFunctions.Input)
        {
            ExecuteInput();
            return;
        }
        
        if (callExpression.Function == BuiltInFunctions.Random)
        {
            ExecuteRandom(callExpression.Arguments);
            return;
        }

        var frame = new VariableDictionary();
        for (int i = 0; i < callExpression.Arguments.Length; i++)
        {
            Visit(callExpression.Arguments[i]);
            frame.Add(callExpression.Function.Parameters[i], Result);
        }

        if (!functions.ContainsKey(callExpression.Function))
            throw EvaluationException.MissingMethod(callExpression.Function.Name);
        locals.Push(frame);
        Visit(functions[callExpression.Function]);
        if (callExpression.Function.ReturnType == TypeSymbol.Void)
            Result = null;
        locals.Pop();
    }
    protected override void VisitBoundConversionExpression(BoundConversionExpression conversionExpression)
    {
        Visit(conversionExpression.Expression);
        if (conversionExpression.Type == TypeSymbol.String)
            Result = Convert.ToString(Result, CultureInfo.InvariantCulture);
        else if (conversionExpression.Type == TypeSymbol.Integer)
            Result = Convert.ToInt32(Result, CultureInfo.InvariantCulture);
        else if (conversionExpression.Type == TypeSymbol.Boolean)
            Result = Convert.ToBoolean(Result, CultureInfo.InvariantCulture);
        else if (conversionExpression.Type != TypeSymbol.Any)
            throw EvaluationException.InvalidCast(conversionExpression.Expression.Type, conversionExpression.Type);
    }
    void ExecutePrint(IEnumerable<BoundExpression> arguments, bool line)
    {
        Visit(arguments.Single());
        string argument = Result?.ToString() ?? string.Empty;
        Console.Out.WriteColoredText(argument, ConsoleColor.White);
        if (line) Console.Out.WriteLine();
        Result = null;
    }
    void ExecuteInput()
    {
        Result = Console.ReadLine();
    }

    void ExecuteRandom(IEnumerable<BoundExpression> arguments)
    {
        Visit(arguments.Single());
        int maximum = (int)Result!;
        Result = RandomNumberGenerator.GetInt32(0, maximum);
    }

    public static object? Evaluate(BoundProgram program, VariableDictionary globals)
    {
        var functionDictionaryBuilder = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var prg = program;
        while (prg is not null)
        {
            foreach (var (function, body) in prg.Functions)
                functionDictionaryBuilder.TryAdd(function, body);
            prg = prg.Previous;
        }

        var functionDictionary = functionDictionaryBuilder.ToImmutable();
        var evaluator = new Evaluator(globals, functionDictionary);
        evaluator.VisitBoundBlockStatement(program.Functions[program.EntryPoint]);
        return evaluator.Result;
    }
}
