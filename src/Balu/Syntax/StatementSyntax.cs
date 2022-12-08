using System;
using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// The abstract base class for statement <see cref="SyntaxNode">syntax nodes</see>.
/// </summary>
public abstract class StatementSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="BlockStatementSyntax"/> from the given <see cref="SyntaxToken"/> for open and closed braces and the <see cref="StatementSyntax"/> instances of the block.
    /// </summary>
    /// <param name="openBraceToken">The <see cref="SyntaxToken"/> of the open brace.</param>
    /// <param name="statements">The <see cref="StatementSyntax"/> instances representing the body of the block.</param>
    /// <param name="closedBraceToken">The <see cref="SyntaxToken"/> of the closed brace.</param>
    /// <returns>The parsed <see cref="BlockStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
    public static BlockStatementSyntax
        BlockStatement(SyntaxToken openBraceToken, IEnumerable<StatementSyntax> statements, SyntaxToken closedBraceToken) => new(
        openBraceToken ?? throw new ArgumentNullException(nameof(openBraceToken)), statements ?? throw new ArgumentNullException(nameof(statements)),
        closedBraceToken ?? throw new ArgumentNullException(nameof(closedBraceToken)));
    /// <summary>
    /// Creates a new <see cref="ExpressionStatementSyntax"/> from the given <see cref="ExpressionSyntax"/>.
    /// </summary>
    /// <param name="expression">The underlying <see cref="ExpressionSyntax"/> of the statement.</param>
    /// <returns>The parsed <see cref="ExpressionStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
    public static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression) => new(expression ?? throw new ArgumentNullException(nameof(expression)));
    /// <summary>
    /// Creates a new <see cref="VariableDeclarationSyntax"/> from the given elements.
    /// </summary>
    /// <param name="expression">The underlying <see cref="ExpressionSyntax"/> of the statement.</param>
    /// <returns>The parsed <see cref="ExpressionStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static VariableDeclarationSyntax VariableDeclaration(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equals,
                                                                ExpressionSyntax expression) => new(
        keyword ?? throw new ArgumentNullException(nameof(keyword)),
        identifier ?? throw new ArgumentNullException(nameof(identifier)),
        equals ?? throw new ArgumentNullException(nameof(equals)),
        expression ?? throw new ArgumentNullException(nameof(expression)));
}
