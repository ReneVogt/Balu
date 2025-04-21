using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundPostfixExpression(SyntaxNode syntax, VariableSymbol variable, BoundBinaryOperator op) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.PostfixExpression;
    public override TypeSymbol Type => Operator.Type;
    public override bool HasSideEffects => true;

    public VariableSymbol Variable { get; } = variable;
    public BoundBinaryOperator Operator { get; } = op;

    public override string ToString() => $"{Variable.Name}{Operator.SyntaxKind.GetText()}";
}