using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class TypeClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ColonToken;
            yield return Identifier;
        }
    }
    public SyntaxToken ColonToken { get; }
    public SyntaxToken Identifier { get; }

    public TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ColonToken = colonToken ?? throw new ArgumentNullException(nameof(colonToken));
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter)
    {
        var colon = (SyntaxToken)rewriter.Visit(ColonToken);
        var identifier = (SyntaxToken)rewriter.Visit(Identifier);
        return colon == ColonToken && identifier == Identifier ? this : new(SyntaxTree, colon, identifier);
    }
}
