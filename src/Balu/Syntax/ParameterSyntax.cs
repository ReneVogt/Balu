using System;

namespace Balu.Syntax;

public sealed partial class ParameterSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Parameter;
    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax TypeClause { get; }

    internal ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClauseSyntax type) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier)) ;
        TypeClause = type ?? throw new ArgumentNullException(nameof(type));
    }
}
