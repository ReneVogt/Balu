using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a compilation unit, e.g. a Balu code file.
/// </summary>
public sealed class CompilationUnitSyntax : SyntaxNode
{
    /// <summary>
    /// The root statement of this compilation unit.
    /// </summary>
    public StatementSyntax Statement { get; }
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
            yield return Statement;
            yield return EndOfFileToken;
        }
    }

    internal CompilationUnitSyntax(StatementSyntax statement, SyntaxToken endOfFileToken) =>
        (Statement, EndOfFileToken) = (statement, endOfFileToken);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var statement = (StatementSyntax)visitor.Visit(Statement);
        var eof = (SyntaxToken)visitor.Visit(EndOfFileToken);
        return statement != Statement || eof != EndOfFileToken ? CompilationUnit(statement, eof) : this;
    }
}
