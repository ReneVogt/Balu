using System;
using Balu.Binding;

namespace Balu;

sealed class Evaluator : BoundExpressionVisitor
{
    public int Result { get; private set; }

    Evaluator() {}

    protected override BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        Result = (int)literalExpression.Value;
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
                Result = -Result;
                break;
            default:
                throw new InvalidOperationException($"Unary operator {unaryExpression.OperatorKind} cannot be evaluated.");
        }

        return unaryExpression;
    }
    protected override BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);
        int left = Result;
        Visit(binaryExpression.Right);
        int right = Result;
        Result = binaryExpression.OperatorKind switch
        {
            BoundBinaryOperatorKind.Addition => left + right,
            BoundBinaryOperatorKind.Substraction => left - right,
            BoundBinaryOperatorKind.Multiplication => left * right,
            BoundBinaryOperatorKind.Division => left / right,
            _ => throw new InvalidOperationException($"Unary operator {binaryExpression.OperatorKind} cannot be evaluated."),
        };
        return binaryExpression;
    }

    public static int Evaluate(BoundExpression expression)
    {
        var evaluator = new Evaluator();
        evaluator.Visit(expression ?? throw new ArgumentNullException(nameof(expression)));
        return evaluator.Result;
    }
}
