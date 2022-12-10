using System.Collections.Generic;

namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public abstract IEnumerable<BoundNode> Children { get; }
    internal abstract BoundNode Accept(BoundTreeVisitor visitor);

}
