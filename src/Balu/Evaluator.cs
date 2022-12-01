using System;
using Balu.Binding;

namespace Balu;

sealed class Evaluator
{
    readonly BoundExpression root;

    Evaluator(BoundExpression root) => this.root = root ?? throw new ArgumentNullException(nameof(root));

    public int Evaluate() => EvaluateExpression(root);

    static int EvaluateExpression(BoundExpression expression) => expression switch
    {
        BoundLiteralExpression { Value: int number } => number,
        BoundUnaryExpression { OperatorKind: BoundUnaryOperatorKind.Identity, Operand: var operand } => Evaluate(operand),
        BoundUnaryExpression { OperatorKind: BoundUnaryOperatorKind.Negation, Operand: var operand } => -Evaluate(operand),
        BoundBinaryExpression { OperatorKind: BoundBinaryOperatorKind.Addition, Left: var left, Right: var right } => Evaluate(left) + Evaluate(right),
        BoundBinaryExpression { OperatorKind: BoundBinaryOperatorKind.Substraction, Left: var left, Right: var right } => Evaluate(left) - Evaluate(right),
        BoundBinaryExpression { OperatorKind: BoundBinaryOperatorKind.Multiplication, Left: var left, Right: var right } => Evaluate(left) * Evaluate(right),
        BoundBinaryExpression { OperatorKind: BoundBinaryOperatorKind.Division, Left: var left, Right: var right } => Evaluate(left) / Evaluate(right),
        _ => throw new InvalidOperationException($"Expressions {expression} cannot be evaluated.")
    };

    public static int Evaluate(BoundExpression expression) => new Evaluator(expression).Evaluate();
}
