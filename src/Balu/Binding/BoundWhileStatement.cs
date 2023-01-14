using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundWhileStatement : BoundLoopStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

    public BoundExpression Condition { get; }
    public BoundStatement Body { get; }

    public BoundWhileStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : base(syntax, breakLabel, continueLabel)
    {
        Condition = condition;
        Body = body;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var condition = (BoundExpression)rewriter.Visit(Condition);
        var body = (BoundStatement)rewriter.Visit(Body);
        return condition == Condition && body == Body
                   ? this
                   : new (Syntax, condition, body, BreakLabel, ContinueLabel);
    }
}