using Balu.Syntax;

namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public abstract int ChildrenCount { get; }
    public SyntaxNode Syntax { get; }

    private protected BoundNode(SyntaxNode syntax) => Syntax = syntax;

    public abstract BoundNode GetChild(int index);

    public override string ToString() => Kind.ToString();
}
