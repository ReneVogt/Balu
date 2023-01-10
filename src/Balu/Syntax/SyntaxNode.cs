using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

public abstract class SyntaxNode
{
    readonly Lazy<TextSpan> span, fullSpan;

    public SyntaxTree SyntaxTree { get; }
    public abstract SyntaxKind Kind { get; }
    public virtual TextSpan Span => span.Value;
    public virtual TextSpan FullSpan => fullSpan.Value;
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
        fullSpan = new(() =>
        {
            var children = Children.ToArray();
            var first = children.First();
            var last = children.Last();
            return first.FullSpan with { Length = last.FullSpan.End - first.FullSpan.Start };
        });
    }

    internal abstract SyntaxNode Rewrite(SyntaxTreeRewriter rewriter);
    protected static ImmutableArray<T> RewriteList<T>(SyntaxTreeRewriter rewriter, ImmutableArray<T> nodes) where T : SyntaxNode
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
    protected static SeparatedSyntaxList<T> RewriteList<T>(SyntaxTreeRewriter rewriter, SeparatedSyntaxList<T> list) where T : SyntaxNode
    {
        _ = rewriter ?? throw new ArgumentNullException(nameof(rewriter));
        _ = list ?? throw new ArgumentNullException(nameof(list));

        ImmutableArray<SyntaxNode>.Builder? resultBuilder = null;
        for (int i = 0; i < list.ElementsWithSeparators.Length; i++)
        {
            var node = (T)rewriter.Visit(list.ElementsWithSeparators[i]);
            if (node != list.ElementsWithSeparators[i] && resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<SyntaxNode>(list.ElementsWithSeparators.Length);
                resultBuilder.AddRange(list.ElementsWithSeparators.Take(i));
            }
            resultBuilder?.Add(node);
        }

        return resultBuilder is null ? list : new (resultBuilder.ToImmutable());
    }

    internal virtual void Accept(SyntaxTreeVisitor visitor)
    {
        foreach (var child in Children) visitor.Visit(child);
    }

    SyntaxToken GetLastToken() => this as SyntaxToken ?? Children.Last().GetLastToken();

    public override string ToString() => $"{Kind}{Span}";
}
