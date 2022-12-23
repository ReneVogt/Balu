using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

public abstract class SyntaxNode
{
    readonly Lazy<TextSpan> span;

    public SyntaxTree SyntaxTree { get; }
    public abstract SyntaxKind Kind { get; }
    public virtual TextSpan Span => span.Value;
    public abstract IEnumerable<SyntaxNode> Children { get; }
    public SyntaxToken LastToken => GetLastToken();
    public TextLocation Location => new (SyntaxTree.Text, Span);

    private protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
        span = new(() =>
        {
            var children = Children.ToArray();
            var first = children.First();
            var last = children.Last();
            return first.Span with { Length = last.Span.End - first.Span.Start };
        });
    }


    internal abstract SyntaxNode Accept(SyntaxVisitor visitor);

    protected static ImmutableArray<T> VisitList<T>(SyntaxVisitor visitor, ImmutableArray<T> nodes) where T : SyntaxNode
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
    protected static SeparatedSyntaxList<T> VisitList<T>(SyntaxVisitor visitor, SeparatedSyntaxList<T> list) where T : SyntaxNode
    {
        _ = visitor ?? throw new ArgumentNullException(nameof(visitor));
        _ = list ?? throw new ArgumentNullException(nameof(list));

        ImmutableArray<SyntaxNode>.Builder? resultBuilder = null;
        for (int i = 0; i < list.ElementsWithSeparators.Length; i++)
        {
            var node = (T)visitor.Visit(list.ElementsWithSeparators[i]);
            if (node != list.ElementsWithSeparators[i] && resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<SyntaxNode>(list.ElementsWithSeparators.Length);
                resultBuilder.AddRange(list.ElementsWithSeparators.Take(i));
            }
            resultBuilder?.Add(node);
        }

        return resultBuilder is null ? list : new (resultBuilder.ToImmutable());
    }

    SyntaxToken GetLastToken() => this as SyntaxToken ?? Children.Last().GetLastToken();

    public override string ToString() => $"{Kind}{Span}";
}
