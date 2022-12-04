﻿using System;
using System.Collections.Generic;
using Balu.Binding;

namespace Balu.Evaluation;

sealed class Evaluator : BoundExpressionVisitor
{
    readonly Dictionary<string, object?> variables;
    public object? Result { get; private set; }

    Evaluator(Dictionary<string, object?> variables) => this.variables = variables;

    protected override BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        Result = literalExpression.Value;
        return literalExpression;
    }
    protected override BoundExpression VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
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
            default:
                throw EvaluationException.UnaryOperatorCannotBeEvaluated(unaryExpression.Operator.OperatorKind);
        }

        return unaryExpression;
    }
    protected override BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
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
            BoundBinaryOperatorKind.Equals => Equals(left, right),
            BoundBinaryOperatorKind.NotEqual => !Equals(left, right),
            _ => throw EvaluationException.BinaryOperatorCannotBeEvaluated(binaryExpression.Operator.OperatorKind)
        };
        return binaryExpression;
    }
    protected override BoundExpression VisitBoundVariableExpression(BoundVariableExpression variableExpression)
    {
        Result = variables[variableExpression.Name];
        return variableExpression;
    }
    protected override BoundExpression VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression) => base.VisitBoundAssignmentExpression(assignmentExpression);

    public static object? Evaluate(BoundExpression expression, Dictionary<string, object?> variables)
    {
        var evaluator = new Evaluator(variables);
        evaluator.Visit(expression ?? throw new ArgumentNullException(nameof(expression)));
        return evaluator.Result;
    }
}