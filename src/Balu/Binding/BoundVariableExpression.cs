using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override BoundConstant? Constant => Variable.Constant;
    public override TypeSymbol Type => Variable.Type;

    public VariableSymbol Variable { get; }

    public BoundVariableExpression(SyntaxNode syntax, VariableSymbol variable) : base(syntax)
    {
        Variable = variable;
    }
    
    public override string ToString() => Variable.Name;
}