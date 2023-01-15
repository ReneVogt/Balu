using System;

namespace Balu.Syntax;

public sealed partial class TypeClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
    public SyntaxToken ColonToken { get; }
    public SyntaxToken Identifier { get; }

    internal TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ColonToken = colonToken ?? throw new ArgumentNullException(nameof(colonToken));
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }
}
