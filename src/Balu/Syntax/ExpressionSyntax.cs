using System;

namespace Balu.Syntax;

/// <summary>
/// The abstract base class for expresssion <see cref="SyntaxNode">syntax nodes</see>.
/// </summary>
public abstract class ExpressionSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="LiteralExpressionSyntax"/> from the given <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="literalToken">The <see cref="SyntaxToken"/> creating this expression.</param>
    /// <returns>The parsed <see cref="LiteralExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="literalToken"/> is <c>null</c>.</exception>
    public static LiteralExpressionSyntax Literal(SyntaxToken literalToken, object? value = null) => new(literalToken ?? throw new ArgumentNullException(nameof(literalToken)), value);
    /// <summary>
    /// Creates a new <see cref="UnaryExpressionSyntax"/> from the given <paramref name="operatorToken"/> and <paramref name="expression"/>.
    /// </summary>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="expression">The <see cref="ExpressionSyntax"/> representing the operand expression.</param>
    /// <returns>The <see cref="UnaryExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operatorToken"/> or <paramref name="expression"/> is <c>null</c>.</exception>
    public static UnaryExpressionSyntax Unary(SyntaxToken operatorToken, ExpressionSyntax expression) => new(operatorToken ?? throw new ArgumentNullException(nameof(operatorToken)), expression ?? throw new ArgumentNullException(nameof(expression)));
    /// <summary>
    /// Creates a new <see cref="BinaryExpressionSyntax"/> from the given <paramref name="left"/> and <paramref name="right"/> <see cref="ExpressionSyntax"> expressions</see>
    /// and the given <paramref name="operatorToken"/> 
    /// </summary>
    /// <param name="left">The <see cref="ExpressionSyntax"/> representing the left operand.</param>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="right">The <see cref="ExpressionSyntax"/> representing the right operand.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/>, <paramref name="operatorToken"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static BinaryExpressionSyntax Binary(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right) => new(left ?? throw new ArgumentNullException(nameof(left)), operatorToken ?? throw new ArgumentNullException(nameof(operatorToken)), right ?? throw new ArgumentNullException(nameof(right)));
    /// <summary>
    /// Creates a new <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="openParenthesisToken">The <see cref="SyntaxToken"/> representing the opening parenthesis.</param>
    /// <param name="expresssion">The <see cref="ExpressionSyntax"/> representing the inner expression.</param>
    /// <param name="closedParenthesisToken">The <see cref="SyntaxToken"/> representing the closing parenthesis.</param>
    /// <returns>The <see cref="ParenthesizedExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="openParenthesisToken"/>, <paramref name="expresssion"/> or <paramref name="closedParenthesisToken"/> is <c>null</c>.</exception>
    public static ParenthesizedExpressionSyntax Parenthesized(SyntaxToken openParenthesisToken, ExpressionSyntax expresssion, SyntaxToken closedParenthesisToken) => new(openParenthesisToken ?? throw new ArgumentNullException(nameof(openParenthesisToken)), expresssion ?? throw new ArgumentNullException(nameof(expresssion)), closedParenthesisToken ?? throw new ArgumentNullException(nameof(closedParenthesisToken)));
    /// <summary>
    /// Creates a new <see cref="NameExpressionSyntax"/>.
    /// </summary>
    /// <param name="identifierToken">The <see cref="SyntaxToken"/> representing name.</param>
    /// <returns>The <see cref="NameExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifierToken"/> is <c>null</c>.</exception>
    public static NameExpressionSyntax Name(SyntaxToken identifierToken) => new(identifierToken ?? throw new ArgumentNullException(nameof(identifierToken)));
    /// <summary>
    /// Creates a new <see cref="AssignmentExpressionSyntax"/>.
    /// </summary>
    /// <param name="identifierToken">The <see cref="SyntaxToken"/> representing the variable name.</param>
    /// <param name="equalsToken">The <see cref="SyntaxToken"/> representing the equals token.</param>
    /// <param name="expression">The expression of which the result is assigned.</param>
    /// <returns>The <see cref="AssignmentExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifierToken"/>, <paramref name="equalsToken"/> or <paramref name="expression"/> is <c>null</c>.</exception>
    public static AssignmentExpressionSyntax Assignment(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression) => new(identifierToken ?? throw new ArgumentNullException(nameof(identifierToken)), equalsToken ?? throw new ArgumentNullException(nameof(equalsToken)), expression ?? throw new ArgumentNullException(nameof(expression)));
    /// <summary>
    /// Creates a new <see cref="CallExpressionSyntax"/>.
    /// </summary>
    /// <param name="identifierToken">The <see cref="SyntaxToken"/> representing the function's name.</param>
    /// <param name="openParenthesisToken">The <see cref="SyntaxToken"/> representing the opening parenthesis.</param>
    /// <param name="parameters">The comma separated list of arguments.</param>
    /// <param name="closedParenthesisToken">The <see cref="SyntaxToken"/> representing the closing parenthesis.</param>
    /// <returns>The <see cref="CallExpressionSyntax"/>.</returns>
    /// <exception cref="ArgumentNullException">On or more of the arguments are <c>null</c>.</exception>
    public static CallExpressionSyntax Call(SyntaxToken identifierToken, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> parameters, SyntaxToken closedParenthesisToken) => 
        new(identifierToken ?? throw new ArgumentNullException(nameof(identifierToken)), 
            openParenthesisToken ?? throw new ArgumentNullException(nameof(openParenthesisToken)), 
            parameters ?? throw new ArgumentNullException(nameof(parameters)),
            closedParenthesisToken ?? throw new ArgumentNullException(nameof(closedParenthesisToken)));
}
