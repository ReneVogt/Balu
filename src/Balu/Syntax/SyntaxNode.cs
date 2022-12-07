using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;

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

    public override string ToString() => Kind.ToString();

    /// <summary>
    /// Creates a new <see cref="CompilationUnitSyntax"/> from the given <see cref="ExpressionSyntax"/>.
    /// </summary>
    /// <param name="expression">The root <see cref="ExpressionSyntax"/> of the compilation unit.</param>
    /// <param name="endOfFileToken">The eof token of the compilation unit.</param>
    /// <returns>A new <see cref="CompilationUnitSyntax"/> instance.</returns>
    public static CompilationUnitSyntax CompilationUnit(ExpressionSyntax expression, SyntaxToken endOfFileToken) =>
        new CompilationUnitSyntax(expression, endOfFileToken);
}
