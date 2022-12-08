namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    internal abstract BoundNode Accept(BoundTreeVisitor visitor);

}
