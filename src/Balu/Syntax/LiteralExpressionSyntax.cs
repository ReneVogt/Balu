using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a literal expression like numbers or identifiers.
/// </summary>
public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The <see cref="SyntaxToken"/> of this expression.
    /// </summary>
    public SyntaxToken LiteralToken { get; }
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return LiteralToken;
        }
    }

    /// <summary>
    /// The value of the <see cref="LiteralExpressionSyntax"/>.
    /// </summary>
    public object? Value { get; }

    internal LiteralExpressionSyntax(SyntaxToken literalToken) : this(literalToken, literalToken.Value){}
    internal LiteralExpressionSyntax(SyntaxToken literalToken, object? value) => (LiteralToken, Value) = (literalToken, value);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        SyntaxToken literal = (SyntaxToken)visitor.Visit(LiteralToken);
        return literal == LiteralToken ? this : Literal(literal, literal.Value);
    }
}
