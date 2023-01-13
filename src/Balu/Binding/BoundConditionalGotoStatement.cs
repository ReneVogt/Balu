using Balu.Syntax;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundLabel Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Condition; }
    }
    
    public BoundConditionalGotoStatement(SyntaxNode syntax, BoundLabel label, BoundExpression condition, bool jumpIfTrue = true) : base(syntax)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var condition = (BoundExpression)rewriter.Visit(Condition);
        return condition == Condition ? this : new (Syntax, Label, condition, JumpIfTrue);
    }

    public override string ToString() => $"goto {Label} {(JumpIfTrue ? "if" : "if not")} {Condition}";
}
