using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;
sealed partial class BoundVariableDeclarationStatement(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Expression { get; } = expression;

    public override string ToString() => $"{(Variable.ReadOnly ? "let" : "var")} {Variable.Name} =  {Expression}";
}