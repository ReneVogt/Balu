using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundWhileStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : BoundLoopStatement(syntax, breakLabel, continueLabel)
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;
}