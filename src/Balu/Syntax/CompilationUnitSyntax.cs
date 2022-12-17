using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

/// <summary>
/// Represents a compilation unit, e.g. a Balu code file.
/// </summary>
public sealed class CompilationUnitSyntax : SyntaxNode
{
    /// <summary>
    /// The member declarations at the top level.
    /// </summary>
    public ImmutableArray<MemberSyntax> Members { get; }
    /// <summary>
    /// The eof token of this compilation unit.
    /// </summary>
    public SyntaxToken EndOfFileToken { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            foreach (var member in Members)
                yield return member;
            yield return EndOfFileToken;
        }
    }

    internal CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken) =>
        (Members, EndOfFileToken) = (members, endOfFileToken);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var members = VisitList(visitor, Members);
        var eof = (SyntaxToken)visitor.Visit(EndOfFileToken);
        return members != Members || eof != EndOfFileToken ? CompilationUnit(members, eof) : this;
    }
}