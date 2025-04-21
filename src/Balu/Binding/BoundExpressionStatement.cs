using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpression Expression { get; } = expression;

    public override string ToString() => Expression.ToString();
}