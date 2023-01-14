using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundReturnStatement : BoundStatement
{
    public BoundExpression? Expression { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

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
