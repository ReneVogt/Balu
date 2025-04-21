using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundGotoStatement(SyntaxNode syntax, BoundLabel label) : BoundStatement(syntax)
{
    public BoundLabel Label { get; } = label;

    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

    public override string ToString() => $"goto {Label}";
}
