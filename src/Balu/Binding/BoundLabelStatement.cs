using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundLabelStatement : BoundStatement
{
    public BoundLabel Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

    public BoundLabelStatement(SyntaxNode syntax, BoundLabel label)
        : base(syntax)
    {
        Label = label;
    }

    public override string ToString() => Label.ToString();
}
