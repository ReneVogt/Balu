using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a function parameter.
/// </summary>
public sealed class ParameterSyntax : SyntaxNode
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.Parameter;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Identifier;
            yield return TypeClause;
        }
    }

    /// <summary>
    /// The <see cref="SyntaxToken"/> for the parameter's name.
    /// </summary>
    public SyntaxToken Identifier { get; }
    /// <summary>
    /// The parameter's type clause.
    /// </summary>
    public TypeClauseSyntax TypeClause { get; }

    internal ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax type)
    {
        Identifier = identifier;
        TypeClause = type;
    }

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        var type = (TypeClauseSyntax)visitor.Visit(TypeClause);
        return identifier == Identifier && type == TypeClause ? this : Parameter(identifier, type);
    }
}
