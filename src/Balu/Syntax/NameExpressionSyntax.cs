using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierrToken { get; }
    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return IdentifierrToken;
        }
    }

    public NameExpressionSyntax(SyntaxTree? syntaxTree, SyntaxToken identifierrToken) : base(syntaxTree)
    { 
        IdentifierrToken = identifierrToken ?? throw new ArgumentNullException(nameof(identifierrToken));
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken identifierToken = (SyntaxToken)visitor.Visit(IdentifierrToken);
        return identifierToken!= IdentifierrToken  ? new(null, identifierToken) : this;
    }
}
