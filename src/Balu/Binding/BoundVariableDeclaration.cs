﻿namespace Balu.Binding;

sealed class BoundVariableDeclaration : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
    
    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundVariableDeclaration(VariableSymbol variable, BoundExpression expression)
    {
        Variable = variable;
        Expression = expression;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var expression = (BoundExpression)visitor.Visit(Expression);
        return expression == Expression ? this : new (Variable, expression);
    }
}