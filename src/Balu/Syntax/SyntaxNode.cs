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
    public abstract int ChildrenCount { get; }
    public SyntaxToken LastToken => GetLastToken();
    public TextLocation Location => new (SyntaxTree.Text, Span);

    private protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
        span = new(() =>
        {
            if (ChildrenCount == 0) return default;
            var first = GetChild(0);
            var last = GetChild(ChildrenCount - 1);
            return first.Span with { Length = last.Span.End - first.Span.Start };
        });
        fullSpan = new(() =>
        {
            if (ChildrenCount == 0) return default;
            var first = GetChild(0);
            var last = GetChild(ChildrenCount - 1);
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

    public abstract SyntaxNode GetChild(int index);

    internal virtual void Accept(SyntaxTreeVisitor visitor)
    {
        for(int i=0; i<ChildrenCount; i++) visitor.Visit(GetChild(i));
    }

    SyntaxToken GetLastToken() => this as SyntaxToken ?? GetChild(ChildrenCount-1).GetLastToken();

    public override string ToString() => $"{Kind}{Span}";
}
