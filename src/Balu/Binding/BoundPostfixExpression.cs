using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundPostfixExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.PostfixExpression;
    public override TypeSymbol Type => Operator.Type;
    public override bool HasSideEffects => true;

    public VariableSymbol Variable { get; }
    public BoundBinaryOperator Operator { get; }

    public BoundPostfixExpression(SyntaxNode syntax, VariableSymbol variable, BoundBinaryOperator op) : base(syntax)
    {
        Variable = variable;
        Operator = op;
    }

    public override string ToString() => $"{Variable.Name}{Operator.SyntaxKind.GetText()}";
}