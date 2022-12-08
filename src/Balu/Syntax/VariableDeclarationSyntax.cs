using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class VariableDeclarationSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children => throw new System.NotImplementedException();

    public SyntaxToken KeywordToken { get; }
    /// <summary>
    /// The identifier token specifying the variable name.
    /// </summary>
    public SyntaxToken IdentifierToken { get; }
    /// <summary>
    /// The equals token of the declaration.
    /// </summary>
    public SyntaxToken EqualsToken { get; }
    /// <summary>
    /// The expression to assign to the variable.
    /// </summary>
    public ExpressionSyntax Expression { get; }

    internal VariableDeclarationSyntax(SyntaxToken keywordToken, SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Expression = expression;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var keyword = (SyntaxToken)visitor.Visit(KeywordToken);
        var identifier = (SyntaxToken)visitor.Visit(IdentifierToken);
        var equals = (SyntaxToken)visitor.Visit(EqualsToken);
        var expression = (ExpressionSyntax)visitor.Visit(Expression);
        return keyword == KeywordToken && identifier == IdentifierToken && equals == EqualsToken && expression == Expression
                   ? this
                   : VariableDeclaration(keyword, identifier, equals, expression);
    }
}