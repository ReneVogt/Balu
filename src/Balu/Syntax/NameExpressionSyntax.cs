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

    internal NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    { 
        IdentifierrToken = identifierrToken ?? throw new ArgumentNullException(nameof(identifierrToken));
    }
}
