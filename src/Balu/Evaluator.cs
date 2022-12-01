using System;
using Balu.Binding;

namespace Balu;

sealed class Evaluator : BoundExpressionVisitor
{
    public object? Result { get; private set; }

    Evaluator() {}

    protected override BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        Result = literalExpression.Value;
        return literalExpression;
    }
    protected override BoundExpression VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        Visit(unaryExpression.Operand);
        switch (unaryExpression.OperatorKind)
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
                throw new InvalidOperationException($"Unary operator {unaryExpression.OperatorKind} cannot be evaluated.");
        }

        return unaryExpression;
    }
    protected override BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);
        object left = Result!;
        Visit(binaryExpression.Right);
        object right = Result!;
        Result = binaryExpression.OperatorKind switch
        {
            BoundBinaryOperatorKind.Addition => (int)left + (int)right,
            BoundBinaryOperatorKind.Substraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            _ => throw new InvalidOperationException($"Unary operator {binaryExpression.OperatorKind} cannot be evaluated."),
        };
        return binaryExpression;
    }

    public static object? Evaluate(BoundExpression expression)
    {
        var evaluator = new Evaluator();
        evaluator.Visit(expression ?? throw new ArgumentNullException(nameof(expression)));
        return evaluator.Result;
    }
}
