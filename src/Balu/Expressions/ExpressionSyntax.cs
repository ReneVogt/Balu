namespace Balu.Expressions;

/// <summary>
/// The abstract base class for expresssion <see cref="SyntaxNode">syntax nodes</see>.
/// </summary>
public abstract class ExpressionSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="NumberExpressionSyntax"/> from the given <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="numberToken">The <see cref="SyntaxToken"/> creating this expression.</param>
    /// <returns>The parsed <see cref="NumberExpressionSyntax"/>.</returns>
    public NumberExpressionSyntax NumberExpression(SyntaxToken numberToken) => new(numberToken);
    /// <summary>
    /// Creates a new <see cref="BinaryExpressionSyntax"/> from the given <paramref name="left"/> and <paramref name="right"/> <see cref="ExpressionSyntax"/> expressions
    /// and the given <paramref name="operatorToken"/> 
    /// </summary>
    /// <param name="left">The <see cref="ExpressionSyntax"/> representing the left operand.</param>
    /// <param name="operatorToken">The <see cref="SyntaxToken"/> representing the operator.</param>
    /// <param name="right">The <see cref="ExpressionSyntax"/> representing the right operand.</param>
    /// <returns>The <see cref="BinaryExpressionSyntax"/>.</returns>
    public BinaryExpressionSyntax BinaryExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right) => new(left, operatorToken, right);
}
