using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierrToken { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return IdentifierrToken;
        }
    }

    internal NameExpressionSyntax(SyntaxToken identifierrToken) =>
        IdentifierrToken = identifierrToken;

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken identifierToken = (SyntaxToken)visitor.Visit(IdentifierrToken);
        return identifierToken!= IdentifierrToken  ? Name(identifierToken) : this;
    }
}
