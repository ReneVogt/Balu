using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundVariableExpression(SyntaxNode syntax, VariableSymbol variable) : BoundExpression(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override BoundConstant? Constant => Variable.Constant;
    public override TypeSymbol Type => Variable.Type;

    public VariableSymbol Variable { get; } = variable;

    public override string ToString() => Variable.Name;
}