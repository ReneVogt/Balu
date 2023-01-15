using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundAssignmentExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    public override BoundConstant? Constant => Expression.Constant;
    public override bool HasSideEffects => true;
    public override TypeSymbol Type => Expression.Type;

    public VariableSymbol Symbol { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(SyntaxNode syntax, VariableSymbol symbol, BoundExpression expression) : base(syntax)
    {
        Symbol = symbol;
        Expression = expression;
    }

    public override string ToString() => $"{Symbol.Name} = {Expression}";
}