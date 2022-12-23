﻿using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
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
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }
    public TypeClauseSyntax? TypeClause { get; }

    public VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keywordToken, SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression, TypeClauseSyntax? typeClause)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        KeywordToken = keywordToken ?? throw new ArgumentNullException(nameof(keywordToken));
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
        EqualsToken = equalsToken ?? throw new ArgumentNullException(nameof(equalsToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
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
                   : new(SyntaxTree, keyword, identifier, equals, expression, typeClause);
    }
}