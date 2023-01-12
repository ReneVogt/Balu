﻿using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ParameterSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Parameter;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Identifier;
            yield return TypeClause;
        }
    }
    public SyntaxToken Identifier { get; }
    public TypeClauseSyntax TypeClause { get; }

    internal ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClauseSyntax type) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier)) ;
        TypeClause = type ?? throw new ArgumentNullException(nameof(type));
    }
}
