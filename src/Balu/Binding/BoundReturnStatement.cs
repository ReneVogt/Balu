using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundReturnStatement(SyntaxNode syntax, BoundExpression? expression) : BoundStatement(syntax)
{
    public BoundExpression? Expression { get; } = expression;

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

    public override string ToString() => $"return {Expression}";
}
