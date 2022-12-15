using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Balu.Binding;
using Balu.Symbols;

namespace Balu.Evaluation;

sealed class Evaluator : BoundTreeVisitor, IDisposable
{
    readonly VariableDictionary variables;
    readonly Dictionary<BoundLabel, int> labelsToIndices = new();
    readonly RNGCryptoServiceProvider rng = new();

    public object? Result { get; private set; }

    Evaluator(VariableDictionary variables) => this.variables = variables;
    public void Dispose() => rng.Dispose();

    protected override BoundNode VisitBoundBlockStatement(BoundBlockStatement blockStatement)
    {
        labelsToIndices.Clear();
        for (int i = 0; i < blockStatement.Statements.Length; i++)
            if (blockStatement.Statements[i] is BoundLabelStatement { Label: var label })
                labelsToIndices[label] = i;

        var index = 0;
        while(index < blockStatement.Statements.Length)
        {
            var current = blockStatement.Statements[index];
            switch (current.Kind)
            {
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

        return blockStatement;
    }

    protected override BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        Result = literalExpression.Value;
        return literalExpression;
    }
    protected override BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
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

        return unaryExpression;
    }
    protected override BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
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
        return binaryExpression;
    }
    protected override BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression)
    {
        Result = variables[variableExpression.Variable];
        return variableExpression;
    }
    protected override BoundNode VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression)
    {
        Visit(assignmentExpression.Expression);
        variables[assignmentExpression.Symbol] = Result;
        return assignmentExpression;
    }
    protected override BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        Visit(variableDeclarationStatement.Expression);
        variables[variableDeclarationStatement.Variable] = Result;
        return variableDeclarationStatement;
    }
    protected override BoundNode VisitBoundCallExpression(BoundCallExpression callExpression)
    {
        if (callExpression.Function == BuiltInFunctions.Print)
            ExecutePrint(callExpression.Arguments);
        else if (callExpression.Function == BuiltInFunctions.Input)
            ExecuteInput();
        else if (callExpression.Function == BuiltInFunctions.Random)
            ExecuteRandom(callExpression.Arguments);
        else
            throw EvaluationException.UndefinedMethod(callExpression.Function.Name);
        return callExpression;
    }
    protected override BoundNode VisitBoundConversionExpression(BoundConversionExpression conversionExpression)
    {
        Visit(conversionExpression.Expression);
        if (conversionExpression.Type == TypeSymbol.String)
            Result = Convert.ToString(Result, CultureInfo.InvariantCulture);
        else if (conversionExpression.Type == TypeSymbol.Integer)
            Result = Convert.ToInt32(Result, CultureInfo.InvariantCulture);
        else if (conversionExpression.Type == TypeSymbol.Boolean)
            Result = Convert.ToBoolean(Result, CultureInfo.InvariantCulture);
        else
            throw EvaluationException.InvalidCast(conversionExpression.Expression.Type, conversionExpression.Type);
        return conversionExpression;
    }
    void ExecutePrint(IEnumerable<BoundExpression> arguments)
    {
        Visit(arguments.Single());
        string argument = (string)Result!;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(argument);
        Console.ResetColor();
    }
    void ExecuteInput()
    {
        Result = Console.ReadLine();
    }

    readonly byte[] randomArray = new byte[4];
    void ExecuteRandom(IEnumerable<BoundExpression> arguments)
    {
        Visit(arguments.Single());
        int maximum = (int)Result!;
        rng.GetBytes(randomArray);
        int random = BitConverter.ToInt32(randomArray, 0);
        Result = (random & 0x7FFFFFFF) % maximum; // TODO: avoid modulo bias
    }

    public static object? Evaluate(BoundBlockStatement statement, VariableDictionary variables)
    {
        using var evaluator = new Evaluator(variables);
        evaluator.VisitBoundBlockStatement(statement ?? throw new ArgumentNullException(nameof(statement)));
        return evaluator.Result;
    }
}
