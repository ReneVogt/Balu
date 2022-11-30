using System.Collections.Generic;

namespace Balu.Expressions;

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

    internal LiteralExpressionSyntax(SyntaxToken literalToken) => LiteralToken = literalToken;
}
