using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundDoWhileStatement(SyntaxNode syntax, BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel) : BoundLoopStatement(syntax, breakLabel, continueLabel)
{
    public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

    public BoundStatement Body { get; } = body;
    public BoundExpression Condition { get; } = condition;
}