using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundConditionalGotoStatement(SyntaxNode syntax, BoundLabel label, BoundExpression condition, bool jumpIfTrue = true) : BoundStatement(syntax)
{
    public BoundLabel Label { get; } = label;
    public BoundExpression Condition { get; } = condition;
    public bool JumpIfTrue { get; } = jumpIfTrue;

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

    public override string ToString() => $"goto {Label} {(JumpIfTrue ? "if" : "if not")} {Condition}";
}
