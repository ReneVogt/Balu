using System;

namespace Balu.Syntax;

/// <summary>
/// Abstract base class for member syntax.
/// </summary>
public abstract class MemberSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="GlobalStatementSyntax"/> from the given <see cref="StatementSyntax"/>.
    /// </summary>
    /// <param name="statement">The root <see cref="StatementSyntax"/> of the compilation unit.</param>
    /// <returns>A new <see cref="GlobalStatementSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="statement"/>  is <c>null</c>.</exception>
    public static GlobalStatementSyntax GlobalStatement(StatementSyntax statement) =>
        new(statement ?? throw new ArgumentNullException(nameof(statement)));

}
