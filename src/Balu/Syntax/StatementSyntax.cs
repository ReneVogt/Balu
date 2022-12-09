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

    /// <summary>
    /// Creates a new <see cref="ElseClauseSyntax"/> from the given elements.
    /// </summary>
    /// <param name="ifKeyword">The <see cref="SyntaxToken"/> of the 'if' keyword.</param>
    /// <param name="condition">The <see cref="ExpressionSyntax"/> for the condition.</param>
    /// <param name="thenStatement">The <see cref="StatementSyntax"/> to use when the <paramref name="condition"/> is true.</param>
    /// <param name="elseClause">An optional 'else' part.</param>
    /// <returns>The parsed <see cref="IfStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c> (except for <paramref name="elseClause"/>).</exception>
    public static IfStatementSyntax
        IfStatement(SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause) => new(
        ifKeyword ?? throw new ArgumentNullException(nameof(ifKeyword)), condition ?? throw new ArgumentNullException(nameof(condition)),
        thenStatement ?? throw new ArgumentNullException(nameof(thenStatement)), elseClause);
    /// <summary>
    /// Creates a new <see cref="ElseClauseSyntax"/> from the given elements.
    /// </summary>
    /// <param name="elseKeyword">The <see cref="SyntaxToken"/> of the 'else' keyword.</param>
    /// <param name="statement">The <see cref="StatementSyntax"/> for the 'else' part.</param>
    /// <returns>The parsed <see cref="ElseClauseSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static ElseClauseSyntax Else(SyntaxToken elseKeyword, StatementSyntax statement) => new(
        elseKeyword ?? throw new ArgumentNullException(nameof(elseKeyword)), statement ?? throw new ArgumentNullException(nameof(statement)));
}
