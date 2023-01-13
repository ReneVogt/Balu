using Balu.Syntax;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundExpressionStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Expression;
        }
    }

    public BoundExpression Expression { get; }

    public BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression) : base(syntax)
    {
        Expression = expression;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var expression = (BoundExpression)rewriter.Visit(Expression);
        return expression == Expression ? this : new (Syntax, expression);
    }

    public override string ToString() => Expression.ToString();
}