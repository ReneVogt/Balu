using Balu.Binding;
using Balu.Symbols;

#pragma warning disable CA1032

namespace Balu.Evaluation;

/// <summary>
/// An exception that was raised by <see cref="Evaluator"/>.
/// </summary>
public sealed class EvaluationException : BaluException
{
    internal EvaluationException(string message)
    : base(message) { }

    internal static EvaluationException UnaryOperatorCannotBeEvaluated(BoundUnaryOperatorKind operatorKind) => new ($"Unary operator {operatorKind} cannot be evaluated.");
    internal static EvaluationException BinaryOperatorCannotBeEvaluated(BoundBinaryOperatorKind operatorKind) => new ($"Binary operator {operatorKind} cannot be evaluated.");
    internal static EvaluationException UndefinedMethod(string name) => new($"Method '{name}' is not defined.");

    public static EvaluationException InvalidCast(TypeSymbol fromType, TypeSymbol toType) => new($"Invalid cast from '{fromType}' to '{toType}'.");
}
