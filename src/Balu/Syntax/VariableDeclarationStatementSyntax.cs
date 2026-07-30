using System;

namespace Balu.Syntax;

public sealed partial class VariableDeclarationStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    public TypeClauseSyntax? TypeClause { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }

    internal VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keywordToken, SyntaxToken identifierToken, TypeClauseSyntax? typeClause, SyntaxToken equalsToken, ExpressionSyntax expression)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        KeywordToken = keywordToken ?? throw new ArgumentNullException(nameof(keywordToken));
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
        TypeClause = typeClause;
        EqualsToken = equalsToken ?? throw new ArgumentNullException(nameof(equalsToken));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }
}
