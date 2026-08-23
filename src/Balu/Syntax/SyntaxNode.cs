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
    public SyntaxToken FirstToken => GetFirstToken();
    public SyntaxToken LastToken => GetLastToken();
    public TextLocation Location => new (SyntaxTree.Text, Span);

    private protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
        span = new(() =>
        {
            if (ChildrenCount == 0) return default;
            var first = GetFirstToken();
            var last = GetLastToken();
            return first.Span + last.Span;
        });
        fullSpan = new(() =>
        {
            if (ChildrenCount == 0) return default;
            var first = GetFirstToken();
            var last = GetLastToken();
            return first.FullSpan + last.FullSpan;
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

    SyntaxToken GetLastToken() => GetLastToken(1);
    SyntaxToken GetLastToken(int recursionDepth)
    {
        StackGuard.EnsureSufficientExecutionStack(recursionDepth);
        return this as SyntaxToken ?? GetChild(ChildrenCount - 1).GetLastToken(recursionDepth + 1);
    }
    SyntaxToken GetFirstToken() => GetFirstToken(1);
    SyntaxToken GetFirstToken(int recursionDepth)
    {
        StackGuard.EnsureSufficientExecutionStack(recursionDepth);
        return this as SyntaxToken ?? GetChild(0).GetFirstToken(recursionDepth + 1);
    }

    public override string ToString() => $"{Kind}{Span}";
}
