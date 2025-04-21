using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundLabelStatement(SyntaxNode syntax, BoundLabel label) : BoundStatement(syntax)
{
    public BoundLabel Label { get; } = label;

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

    public override string ToString() => Label.ToString();
}
