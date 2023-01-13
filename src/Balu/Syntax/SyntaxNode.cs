using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

public abstract class SyntaxNode
{
    readonly Lazy<TextSpan> span, fullSpan;

    public SyntaxTree SyntaxTree { get; }
    public SyntaxNode? Parent => SyntaxTree.GetParent(this);

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

    public IEnumerable<SyntaxNode> AncestorsAndSelf()
    {
        var node = this;
        while (node is not null)
        {
            yield return node;
            node = node.Parent;
        }
    }
    public IEnumerable<SyntaxNode> Ancestors() => AncestorsAndSelf().Skip(1);

    internal virtual void Accept(SyntaxTreeVisitor visitor)
    {
        foreach (var child in Children) visitor.Visit(child);
    }

    SyntaxToken GetLastToken() => this as SyntaxToken ?? Children.Last().GetLastToken();

    public override string ToString() => $"{Kind}{Span}";
}
