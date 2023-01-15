using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;
sealed partial class BoundVariableDeclarationStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundVariableDeclarationStatement(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression) : base(syntax)
    {
        Variable = variable;
        Expression = expression;
    }

    public override string ToString() => $"{(Variable.ReadOnly ? "let" : "var")} {Variable.Name} =  {Expression}";
}