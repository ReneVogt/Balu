using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundPrefixExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.PrefixExpression;
    public override TypeSymbol Type => Operator.Type;
    public override bool HasSideEffects => true;

    public BoundBinaryOperator Operator { get; }
    public VariableSymbol Variable { get; }

    public BoundPrefixExpression(SyntaxNode syntax, BoundBinaryOperator op, VariableSymbol variable) : base(syntax)
    {
        Operator = op;
        Variable = variable;
    }

    public override string ToString() => $"{Operator.SyntaxKind.GetText()}{Variable.Name}";
}