using System;

namespace Balu.Syntax;

public sealed partial class NameExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierrToken { get; }
    public override SyntaxKind Kind => SyntaxKind.NameExpression;

    internal NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierrToken) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    { 
        IdentifierrToken = identifierrToken ?? throw new ArgumentNullException(nameof(identifierrToken));
    }
}
