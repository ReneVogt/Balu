using System;
using System.Collections.Immutable;
using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }
    public abstract int ChildrenCount { get; }
    public SyntaxNode Syntax { get; }

    private protected BoundNode(SyntaxNode syntax) => Syntax = syntax;

    public abstract BoundNode GetChild(int index);

    internal abstract BoundNode Rewrite(BoundTreeRewriter rewriter);

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
