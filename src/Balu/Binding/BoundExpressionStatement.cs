using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundExpressionStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpression Expression { get; }

    public BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression) : base(syntax)
    {
        Expression = expression;
    }

    public override string ToString() => Expression.ToString();
}