using System.Collections.Generic;

namespace Balu.Expressions;

public sealed class NumberExpressionSyntax : ExpressionSyntax
{
    /// <summary>
    /// The <see cref="SyntaxToken"/> of this expression.
    /// </summary>
    public SyntaxToken NumberToken { get; }
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.NumberExpression;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return NumberToken;
        }
    }

    internal NumberExpressionSyntax(SyntaxToken numberToken) => NumberToken = numberToken;
}
