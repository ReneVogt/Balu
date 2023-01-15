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

    public override string ToString() => $"return {Expression}";
}
