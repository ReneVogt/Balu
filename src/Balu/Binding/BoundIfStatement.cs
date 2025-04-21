using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundIfStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

    public BoundExpression Condition { get; } = condition;
    public BoundStatement ThenStatement { get; } = thenStatement;
    public BoundStatement? ElseStatement { get; } = elseStatement;
}