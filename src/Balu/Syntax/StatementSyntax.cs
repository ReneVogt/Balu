using System;
using System.Collections.Immutable;

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
        BlockStatement(SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closedBraceToken) => new(
        openBraceToken ?? throw new ArgumentNullException(nameof(openBraceToken)), statements,
        closedBraceToken ?? throw new ArgumentNullException(nameof(closedBraceToken)));
    /// <summary>
    /// Creates a new <see cref="ExpressionStatementSyntax"/> from the given <see cref="ExpressionSyntax"/>.
    /// </summary>
    /// <param name="expression">The underlying <see cref="ExpressionSyntax"/> of the statement.</param>
    /// <returns>The parsed <see cref="ExpressionStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
    public static ExpressionStatementSyntax ExpressionStatement(ExpressionSyntax expression) => new(expression ?? throw new ArgumentNullException(nameof(expression)));
    /// <summary>
    /// Creates a new <see cref="VariableDeclarationStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="keyword">The 'let' or 'var' keyword token.</param>
    /// <param name="identifier">The variable name identifier token.</param>
    /// <param name="equals">The '=' token.</param>
    /// <param name="expression">The underlying <see cref="ExpressionSyntax"/> of the statement.</param>
    /// <param name="typeClause">The optional type clause.</param>
    /// <returns>The parsed <see cref="ExpressionStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument (except <paramref name="typeClause"/>) is <c>null</c>.</exception>
    public static VariableDeclarationStatementSyntax VariableDeclarationStatement(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equals,
                                                                ExpressionSyntax expression, TypeClauseSyntax? typeClause) => new(
        keyword ?? throw new ArgumentNullException(nameof(keyword)),
        identifier ?? throw new ArgumentNullException(nameof(identifier)),
        equals ?? throw new ArgumentNullException(nameof(equals)),
        expression ?? throw new ArgumentNullException(nameof(expression)),
        typeClause);

    /// <summary>
    /// Creates a new <see cref="IfStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="ifKeyword">The <see cref="SyntaxToken"/> of the 'if' keyword.</param>
    /// <param name="condition">The <see cref="ExpressionSyntax"/> for the condition.</param>
    /// <param name="thenStatement">The <see cref="StatementSyntax"/> to use when the <paramref name="condition"/> is true.</param>
    /// <param name="elseClause">An optional 'else' part.</param>
    /// <returns>The parsed <see cref="IfStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c> (except for <paramref name="elseClause"/>).</exception>
    public static IfStatementSyntax
        IfStatement(SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause = null) => new(
        ifKeyword ?? throw new ArgumentNullException(nameof(ifKeyword)), condition ?? throw new ArgumentNullException(nameof(condition)),
        thenStatement ?? throw new ArgumentNullException(nameof(thenStatement)), elseClause);
    /// <summary>
    /// Creates a new <see cref="WhileStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="whileKeyword">The <see cref="SyntaxToken"/> of the 'while' keyword.</param>
    /// <param name="condition">The <see cref="ExpressionSyntax"/> for the condition.</param>
    /// <param name="body">The <see cref="StatementSyntax"/> to execute while the <paramref name="condition"/> is true.</param>
    /// <returns>The parsed <see cref="WhileStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static WhileStatementSyntax
        WhileStatement(SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body) => new(
        whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword)), condition ?? throw new ArgumentNullException(nameof(condition)),
        body ?? throw new ArgumentNullException(nameof(body)));

    /// <summary>
    /// Creates a new <see cref="DoWhileStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="doKeyword">The <see cref="SyntaxToken"/> of the 'do' keyword.</param>
    /// <param name="body">The <see cref="StatementSyntax"/> to execute while the <paramref name="condition"/> is true.</param>
    /// <param name="whileKeyword">The <see cref="SyntaxToken"/> of the 'while' keyword.</param>
    /// <param name="condition">The <see cref="ExpressionSyntax"/> for the condition.</param>
    /// <returns>The parsed <see cref="DoWhileStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static DoWhileStatementSyntax
        DoWhileStatement(SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword, ExpressionSyntax condition) => new(
        doKeyword ?? throw new ArgumentNullException(nameof(doKeyword)),
        body ?? throw new ArgumentNullException(nameof(body)),
        whileKeyword ?? throw new ArgumentNullException(nameof(whileKeyword)), condition ?? throw new ArgumentNullException(nameof(condition)));

    /// <summary>
    /// Creates a new <see cref="ForStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="forKeyword">The <see cref="SyntaxToken"/> of the 'for' keyword.</param>
    /// <param name="identifier">The <see cref="SyntaxToken"/> of the loop variable.</param>
    /// <param name="equals">The <see cref="SyntaxToken"/> of equals sign.</param>
    /// <param name="lowerBound">The <see cref="ExpressionSyntax"/> for the lower bound.</param>
    /// <param name="toKeyword">The <see cref="SyntaxToken"/> of the 'to' keyword.</param>
    /// <param name="upperBound">The <see cref="ExpressionSyntax"/> for the upper bound.</param>
    /// <param name="body">The <see cref="StatementSyntax"/> to execute in the loop.</param>
    /// <returns>The parsed <see cref="ForStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static ForStatementSyntax
        ForStatement(SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken equals, ExpressionSyntax lowerBound, SyntaxToken toKeyword, ExpressionSyntax upperBound, StatementSyntax body) => new(
        forKeyword ?? throw new ArgumentNullException(nameof(forKeyword)),
        identifier ?? throw new ArgumentNullException(nameof(identifier)),
        equals ?? throw new ArgumentNullException(nameof(equals)),
        lowerBound ?? throw new ArgumentNullException(nameof(lowerBound)),
        toKeyword ?? throw new ArgumentNullException(nameof(toKeyword)),
        upperBound ?? throw new ArgumentNullException(nameof(upperBound)),
        body ?? throw new ArgumentNullException(nameof(body)));

    /// <summary>
    /// Creates a new <see cref="ContinueStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="continueKeyword">The <see cref="SyntaxToken"/> of the 'continue' keyword.</param>
    /// <returns>The parsed <see cref="ContinueStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="continueKeyword"/> is <c>null</c>.</exception>
    public static ContinueStatementSyntax
        ContinueStatement(SyntaxToken continueKeyword) => new(continueKeyword ?? throw new ArgumentNullException(nameof(continueKeyword)));
    /// <summary>
    /// Creates a new <see cref="BreakStatementSyntax"/> from the given elements.
    /// </summary>
    /// <param name="breakKeyword">The <see cref="SyntaxToken"/> of the 'break' keyword.</param>
    /// <returns>The parsed <see cref="ContinueStatementSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="breakKeyword"/> is <c>null</c>.</exception>
    public static BreakStatementSyntax
        BreakStatement(SyntaxToken breakKeyword) => new(breakKeyword ?? throw new ArgumentNullException(nameof(breakKeyword)));
}
