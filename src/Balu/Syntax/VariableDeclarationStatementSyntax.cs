﻿using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return KeywordToken;
            yield return IdentifierToken;
            yield return EqualsToken;
            yield return Expression;
            if (TypeClause != null) yield return TypeClause;
        }
    }

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
    /// <summary>
    /// The optional type clause.
    /// </summary>
    public TypeClauseSyntax? TypeClause { get; }

    internal VariableDeclarationStatementSyntax(SyntaxToken keywordToken, SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression, TypeClauseSyntax? typeClause)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Expression = expression;
        TypeClause = typeClause;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var keyword = (SyntaxToken)visitor.Visit(KeywordToken);
        var identifier = (SyntaxToken)visitor.Visit(IdentifierToken);
        var equals = (SyntaxToken)visitor.Visit(EqualsToken);
        var expression = (ExpressionSyntax)visitor.Visit(Expression);
        var typeClause = TypeClause is null ? null : (TypeClauseSyntax)visitor.Visit(TypeClause);
        return keyword == KeywordToken && identifier == IdentifierToken && equals == EqualsToken && expression == Expression
                   ? this
                   : VariableDeclarationStatement(keyword, identifier, equals, expression, typeClause);
    }
}