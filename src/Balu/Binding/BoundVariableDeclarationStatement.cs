using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundVariableDeclarationStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Expression;
        }
    }

    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression expression)
    {
        Variable = variable;
        Expression = expression;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new (Variable, expression);
    }

    public override string ToString() => $"{Kind} \"{Variable.Name}\" ({Expression.Type.Name})";
}