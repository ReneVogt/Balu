using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balu.Visualization;

namespace Balu.Syntax;

/// <summary>
/// A node of a <see cref="SyntaxTree"/>.
/// </summary>
public abstract class SyntaxNode
{
    readonly Lazy<TextSpan> span;

    private protected SyntaxNode()
    {
        span = new(() =>
        {
            var children = Children.ToArray();
            var first = children.First();
            var last = children.Last();
            return new(first.Span.Start, last.Span.End - first.Span.Start);

        });
    }

    /// <summary>
    /// The <see cref="SyntaxKind"/> of this node.
    /// </summary>
    public abstract SyntaxKind Kind { get; }
    /// <summary>
    /// The <see cref="TextSpan"/> of this token in the input stream.
    /// </summary>
    public virtual TextSpan Span => span.Value;
    /// <summary>
    /// Enumerates the child nodes of this <see cref="SyntaxNode"/>.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    internal abstract SyntaxNode Accept(SyntaxVisitor visitor);

    /// <summary>
    /// Writes this <see cref="SyntaxNode"/> into a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <c>null</c>.</exception>
    public void WriteTo(TextWriter writer)
    {
        SyntaxTreePrinter.Print(this, writer ?? throw new ArgumentNullException(nameof(writer)));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
