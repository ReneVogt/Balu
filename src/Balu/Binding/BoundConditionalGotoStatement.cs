using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundConditionalGotoStatement : BoundStatement
{
    public BoundLabel Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    
    public BoundConditionalGotoStatement(SyntaxNode syntax, BoundLabel label, BoundExpression condition, bool jumpIfTrue = true) : base(syntax)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }

    public override string ToString() => $"goto {Label} {(JumpIfTrue ? "if" : "if not")} {Condition}";
}
