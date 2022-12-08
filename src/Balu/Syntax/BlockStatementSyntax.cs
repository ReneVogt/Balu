using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// A block statement surrounded by braces.
/// </summary>
public sealed class BlockStatementSyntax : StatementSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return OpenBraceToken;
            foreach (var statement in Statements) yield return statement;
            yield return ClosedBraceToken;
        }
    }

    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<StatementSyntax> Statements { get; }
    public SyntaxToken ClosedBraceToken { get; }

    internal BlockStatementSyntax(SyntaxToken openBraceToken, IEnumerable<StatementSyntax> statements, SyntaxToken closedBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements.ToImmutableArray();
        ClosedBraceToken = closedBraceToken;
    }


    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var originals = Children.ToList();
        var transformed = originals.Select(visitor.Visit).ToList();
        return originals.SequenceEqual(transformed)
                   ? this
                   : BlockStatement((SyntaxToken)transformed[0], transformed.Skip(1).SkipLast(1).Cast<StatementSyntax>(),
                                    (SyntaxToken)transformed[^1]);
    }
}
