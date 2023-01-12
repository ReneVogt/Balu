using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public ImmutableArray<MemberSyntax> Members { get; }
    public SyntaxToken EndOfFileToken { get; }
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            foreach (var member in Members)
                yield return member;
            yield return EndOfFileToken;
        }
    }

    internal CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        Members = members;
        EndOfFileToken = endOfFileToken ?? throw new ArgumentNullException(nameof(endOfFileToken));
    }
}