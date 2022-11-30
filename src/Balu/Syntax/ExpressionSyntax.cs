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
    public static LiteralExpressionSyntax Literal(SyntaxToken literalToken) => new(literalToken);
    /// <summary>
    /// Creates a new <see cref="UnaryExpressionSyntax"/> from the given <paramref name="operatorToken"/> and <paramref name="expression"/>.
    /// </summary>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="expression">The <see cref="ExpressionSyntax"/> representing the operand expression.</param>
    /// <returns>The <see cref="UnaryExpressionSyntax"/>.</returns>
    public static UnaryExpressionSyntax Unary(SyntaxToken operatorToken, ExpressionSyntax expression) => new(operatorToken, expression);
    /// <summary>
    /// Creates a new <see cref="BinaryExpressionSyntax"/> from the given <paramref name="left"/> and <paramref name="right"/> <see cref="ExpressionSyntax"> expressions</see>
    /// and the given <paramref name="operatorToken"/> 
    /// </summary>
    /// <param name="left">The <see cref="ExpressionSyntax"/> representing the left operand.</param>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="right">The <see cref="ExpressionSyntax"/> representing the right operand.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    public static BinaryExpressionSyntax Binary(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right) => new(left, operatorToken, right);
    /// <summary>
    /// Creates a new <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="openParenthesisToken">The <see cref="SyntaxToken"/> representing the opening parenthesis.</param>
    /// <param name="expresssion">The <see cref="ExpressionSyntax"/> representing the inner expression.</param>
    /// <param name="closedParenthesisToken">The <see cref="SyntaxToken"/> representing the closing parenthesis.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    public static ParenthesizedExpressionSyntax Parenthesized(SyntaxToken openParenthesisToken, ExpressionSyntax expresssion, SyntaxToken closedParenthesisToken) => new(openParenthesisToken, expresssion, closedParenthesisToken);
}
