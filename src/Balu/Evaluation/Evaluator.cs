using System;
using Balu.Binding;

namespace Balu.Evaluation;

sealed class Evaluator : BoundTreeVisitor
{
    readonly VariableDictionary variables;
    public object? Result { get; private set; }

    Evaluator(VariableDictionary variables) => this.variables = variables;

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
            BoundBinaryOperatorKind.Addition => (int)left + (int)right,
            BoundBinaryOperatorKind.Substraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            BoundBinaryOperatorKind.BitwiseAnd => binaryExpression.Type == typeof(bool) ? (bool)left && (bool)right : (int)left & (int)right,
            BoundBinaryOperatorKind.BitwiseOr => binaryExpression.Type == typeof(bool) ? (bool)left || (bool)right : (int)left | (int)right,
            BoundBinaryOperatorKind.BitwiseXor => binaryExpression.Type == typeof(bool) ? (bool)left ^ (bool)right : (int)left ^ (int)right,
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
    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatemnet)
    {
        Visit(ifStatemnet.Condition);
        if ((bool)Result!)
            Visit(ifStatemnet.ThenStatement);
        else if (ifStatemnet.ElseStatement is not null) Visit(ifStatemnet.ElseStatement);
        return ifStatemnet;
    }
    protected override BoundNode VisitBoundWhileStatement(BoundWhileStatement whileStatemnet)
    {
        bool goon;
        do
        {
            Visit(whileStatemnet.Condition);
            goon = (bool)Result!;
            if (goon)
                Visit(whileStatemnet.Body);
        } while (goon);

        return whileStatemnet;
    }
    public static object? Evaluate(BoundStatement statement, VariableDictionary variables)
    {
        var evaluator = new Evaluator(variables);
        evaluator.Visit(statement ?? throw new ArgumentNullException(nameof(statement)));
        return evaluator.Result;
    }
}
