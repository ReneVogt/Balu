using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public abstract IEnumerable<BoundNode> Children { get; }
    internal abstract BoundNode Accept(BoundTreeVisitor visitor);

    protected static ImmutableArray<T> VisitList<T>(BoundTreeVisitor visitor, ImmutableArray<T> nodes) where T : BoundNode
    {
        _ = visitor ?? throw new ArgumentNullException(nameof(visitor));

        ImmutableArray<T>.Builder? resultBuilder = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = (T)visitor.Visit(nodes[i]);
            if (node != nodes[i] && resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<T>(nodes.Length);
                resultBuilder.AddRange(nodes.Take(i));
            }
            resultBuilder?.Add(node);
        }

        return resultBuilder?.ToImmutable() ?? nodes;
    }
    public override string ToString() => Kind.ToString();
}
