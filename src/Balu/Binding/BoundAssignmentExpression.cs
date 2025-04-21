using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundAssignmentExpression(SyntaxNode syntax, VariableSymbol symbol, BoundExpression expression) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    public override BoundConstant? Constant => Expression.Constant;
    public override bool HasSideEffects => true;
    public override TypeSymbol Type => Expression.Type;

    public VariableSymbol Symbol { get; } = symbol;
    public BoundExpression Expression { get; } = expression;

    public override string ToString() => $"{Symbol.Name} = {Expression}";
}