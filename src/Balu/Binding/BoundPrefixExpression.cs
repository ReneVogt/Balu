using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundPrefixExpression(SyntaxNode syntax, BoundBinaryOperator op, VariableSymbol variable) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.PrefixExpression;
    public override TypeSymbol Type => Operator.Type;
    public override bool HasSideEffects => true;

    public BoundBinaryOperator Operator { get; } = op;
    public VariableSymbol Variable { get; } = variable;

    public override string ToString() => $"{Operator.SyntaxKind.GetText()}{Variable.Name}";
}