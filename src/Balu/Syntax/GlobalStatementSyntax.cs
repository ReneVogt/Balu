using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a global statement in the Balu language.
/// </summary>
public sealed class GlobalStatementSyntax : MemberSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get { yield return Statement; }
    }

    /// <summary>
    /// The global statement.
    /// </summary>
    public StatementSyntax Statement { get; }

    internal GlobalStatementSyntax(StatementSyntax statement) => Statement = statement;

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var statement = (StatementSyntax)visitor.Visit(Statement);
        return statement == Statement ? this : GlobalStatement(statement);
    }
}
