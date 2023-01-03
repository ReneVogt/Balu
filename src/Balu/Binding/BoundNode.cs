using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public abstract IEnumerable<BoundNode> Children { get; }
    internal abstract BoundNode Rewrite(BoundTreeRewriter rewriter);

    internal virtual void Accept(BoundTreeVisitor visitor)
    {
        foreach (var child in Children) visitor.Visit(child);
    }

    protected static ImmutableArray<T> RewriteList<T>(BoundTreeRewriter rewriter, ImmutableArray<T> nodes) where T : BoundNode
    {
        _ = rewriter ?? throw new ArgumentNullException(nameof(rewriter));

        ImmutableArray<T>.Builder? resultBuilder = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = (T)rewriter.Visit(nodes[i]);
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
