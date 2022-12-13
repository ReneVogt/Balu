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
    /// <summary>
    /// Returns the last <see cref="SyntaxToken"/> of this <see cref="SyntaxNode"/>.
    /// </summary>
    public SyntaxToken LastToken => GetLastToken();

    internal abstract SyntaxNode Accept(SyntaxVisitor visitor);

    SyntaxToken GetLastToken() => this as SyntaxToken ?? Children.Last().GetLastToken();

    public override string ToString() => $"{Kind}{Span}";


    /// <summary>
    /// Creates a new <see cref="CompilationUnitSyntax"/> from the given <see cref="StatementSyntax"/>.
    /// </summary>
    /// <param name="statement">The root <see cref="StatementSyntax"/> of the compilation unit.</param>
    /// <param name="endOfFileToken">The eof token of the compilation unit.</param>
    /// <returns>A new <see cref="CompilationUnitSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="statement"/> or <paramref name="endOfFileToken"/> is <c>null</c>.</exception>
    public static CompilationUnitSyntax CompilationUnit(StatementSyntax statement, SyntaxToken endOfFileToken) =>
        new (statement ?? throw new ArgumentNullException(nameof(statement)), endOfFileToken ?? throw new ArgumentNullException(nameof(endOfFileToken)));
}
