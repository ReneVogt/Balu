using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundGotoStatement : BoundStatement
{
    public BoundLabel Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

    public BoundGotoStatement(SyntaxNode syntax, BoundLabel label) : base(syntax)
    {
        Label = label;
    }

    public override string ToString() => $"goto {Label}";
}
