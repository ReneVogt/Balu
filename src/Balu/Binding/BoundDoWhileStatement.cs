using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundDoWhileStatement : BoundLoopStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

    public BoundStatement Body { get; }
    public BoundExpression Condition { get; }

    public BoundDoWhileStatement(SyntaxNode syntax, BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel) : base(syntax, breakLabel, continueLabel)
    {
        Body = body;
        Condition = condition;
    }
}