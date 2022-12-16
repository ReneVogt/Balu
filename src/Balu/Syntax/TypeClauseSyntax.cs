using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a type clause (' : type').
/// </summary>
public sealed class TypeClauseSyntax : SyntaxNode
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ColonToken;
            yield return Identifier;
        }
    }

    /// <summary>
    /// The ':' token.
    /// </summary>
    public SyntaxToken ColonToken { get; }
    /// <summary>
    /// The identifier for the type name.
    /// </summary>
    public SyntaxToken Identifier { get; }

    internal TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken identifier) => (ColonToken, Identifier) = (colonToken, identifier);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var colon = (SyntaxToken)visitor.Visit(ColonToken);
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        return colon == ColonToken && identifier == Identifier ? this : Type(colon, identifier);
    }
}
