using System;

namespace Balu.Syntax;

public sealed partial class VariableDeclarationStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }
    public TypeClauseSyntax? TypeClause { get; }

    internal VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keywordToken, SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression, TypeClauseSyntax? typeClause)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        KeywordToken = keywordToken ?? throw new ArgumentNullException(nameof(keywordToken));
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
        EqualsToken = equalsToken ?? throw new ArgumentNullException(nameof(equalsToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        TypeClause = typeClause;
    }
}