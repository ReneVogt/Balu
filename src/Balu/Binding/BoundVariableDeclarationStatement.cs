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

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var expression = (BoundExpression)rewriter.Visit(Expression);
        return expression == Expression ? this : new (Syntax, Variable, expression);
    }

    public override string ToString() => $"{(Variable.ReadOnly ? "let" : "var")} {Variable.Name} =  {Expression}";
}