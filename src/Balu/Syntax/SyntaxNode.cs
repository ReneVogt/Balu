using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    /// <summary>
    /// Creates a new <see cref="CompilationUnitSyntax"/> from the given <see cref="StatementSyntax"/>.
    /// </summary>
    /// <param name="members">The member declarations at the root of the compilation unit.</param>
    /// <param name="endOfFileToken">The eof token of the compilation unit.</param>
    /// <returns>A new <see cref="CompilationUnitSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="members"/> or <paramref name="endOfFileToken"/> is <c>null</c>.</exception>
    public static CompilationUnitSyntax CompilationUnit(IEnumerable<MemberSyntax> members, SyntaxToken endOfFileToken) =>
        new ((members ?? throw new ArgumentNullException(nameof(members))).ToImmutableArray(), endOfFileToken ?? throw new ArgumentNullException(nameof(endOfFileToken)));
    /// <summary>
    /// Creates a new <see cref="ElseClauseSyntax"/> from the given elements.
    /// </summary>
    /// <param name="elseKeyword">The <see cref="SyntaxToken"/> of the 'else' keyword.</param>
    /// <param name="statement">The <see cref="StatementSyntax"/> for the 'else' part.</param>
    /// <returns>The parsed <see cref="ElseClauseSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static ElseClauseSyntax Else(SyntaxToken elseKeyword, StatementSyntax statement) => new(
        elseKeyword ?? throw new ArgumentNullException(nameof(elseKeyword)), statement ?? throw new ArgumentNullException(nameof(statement)));

    /// <summary>
    /// Creates a new <see cref="TypeClauseSyntax"/> from the given elements.
    /// </summary>
    /// <param name="colonToken">The <see cref="SyntaxToken"/> of the ':' token.</param>
    /// <param name="identifier">The <see cref="SyntaxToken"/> for the type name.</param>
    /// <returns>The parsed <see cref="TypeClauseSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static TypeClauseSyntax Type(SyntaxToken colonToken, SyntaxToken identifier) => new(
        colonToken ?? throw new ArgumentNullException(nameof(colonToken)), identifier ?? throw new ArgumentNullException(nameof(identifier)));

    /// <summary>
    /// Creates a new <see cref="ParameterSyntax"/> from the given elements.
    /// </summary>
    /// <param name="identifier">The <see cref="StatementSyntax"/> for the parameter's name.</param>
    /// <param name="type">The <see cref="SyntaxToken"/> for the paramter's type.</param>
    /// <returns>The parsed <see cref="ParameterSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static ParameterSyntax Parameter(SyntaxToken identifier, TypeClauseSyntax type) => new(
        identifier ?? throw new ArgumentNullException(nameof(identifier)), type ?? throw new ArgumentNullException(nameof(type)));
}
