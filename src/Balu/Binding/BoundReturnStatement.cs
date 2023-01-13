using Balu.Syntax;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundReturnStatement : BoundStatement
{
    public BoundExpression? Expression { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            if (Expression is not null) yield return Expression;

        }
    }

    public BoundReturnStatement(SyntaxNode syntax, BoundExpression? expression) : base(syntax)
    {
        Expression = expression;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var expression = Expression is null ? null : (BoundExpression)rewriter.Visit(Expression);
        return expression == Expression ? this : new (Syntax, expression);
    }

    public override string ToString() => $"return {Expression}";
}
