using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// A node of a <see cref="SyntaxTree"/>.
/// </summary>
public abstract class SyntaxNode
{
    /// <summary>
    /// The <see cref="SyntaxKind"/> of this node.
    /// </summary>
    public abstract SyntaxKind Kind { get; }

    /// <summary>
    /// Enumerates the child nodes of this <see cref="SyntaxNode"/>.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> Children { get; }

    internal abstract SyntaxNode Accept(SyntaxVisitor visitor);

    /// <inheritdoc />
    public override string ToString() => $"{Kind}";
}
