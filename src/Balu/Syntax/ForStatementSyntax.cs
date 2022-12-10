using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'for' statement.
/// </summary>
public sealed class ForStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.ForStatement;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ForKeyword;
            yield return IdentifierToken;
            yield return EqualsToken;
            yield return LowerBound;
            yield return ToKeyword;
            yield return UpperBound;
            yield return Body;
        }
    }

    /// <summary>
    /// The <see cref="SyntaxToken"/> representing the 'for' keyword.
    /// </summary>
    public SyntaxToken ForKeyword { get; }
    
    /// <summary>
    /// The <see cref="SyntaxToken"/> for the loop variable.
    /// </summary>
    public SyntaxToken IdentifierToken { get; }
    /// <summary>
    /// The equals token to assign the lower bound.
    /// </summary>
    public SyntaxToken EqualsToken { get; }
    /// <summary>
    /// The <see cref="ExpressionSyntax"/> for the lower bound.
    /// </summary>
    public ExpressionSyntax LowerBound { get; }
    /// <summary>
    /// The 'to' keyword.
    /// </summary>
    public SyntaxToken ToKeyword { get; }
    /// <summary>
    /// The <see cref="ExpressionSyntax"/> for the upper bound.
    /// </summary>
    public ExpressionSyntax UpperBound { get; }

    /// <summary>
    /// The body of the 'for' statement.
    /// </summary>
    public StatementSyntax Body { get; }

    internal ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken identifierToken, SyntaxToken equals, ExpressionSyntax lowerBound, SyntaxToken toKeyWord, ExpressionSyntax upperBound, StatementSyntax body)
    {
        ForKeyword = forKeyword;
        IdentifierToken = identifierToken;
        EqualsToken = equals;
        LowerBound = lowerBound;
        ToKeyword = toKeyWord;
        UpperBound = upperBound;
        Body = body;
    }
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var forKeyword = (SyntaxToken)visitor.Visit(ForKeyword);
        var identifier = (SyntaxToken)visitor.Visit(IdentifierToken);
        var equals = (SyntaxToken)visitor.Visit(EqualsToken);
        var lowerBound = (ExpressionSyntax)visitor.Visit(LowerBound);
        var toKeyWord = (SyntaxToken)visitor.Visit(ToKeyword);
        var upperBound = (ExpressionSyntax)visitor.Visit(UpperBound);
        var body = (StatementSyntax)visitor.Visit(Body);
        return forKeyword == ForKeyword &&
               identifier == IdentifierToken &&
               equals == EqualsToken &&
               lowerBound == LowerBound &&
               toKeyWord == ToKeyword &&
               upperBound == UpperBound &&
               body == Body
                   ? this
                   : ForStatement(forKeyword, identifier, equals, lowerBound, toKeyWord, upperBound, body);
    }
}
