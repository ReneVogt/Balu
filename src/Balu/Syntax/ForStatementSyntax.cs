using System;

namespace Balu.Syntax;

public sealed partial class ForStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ForStatement;
    public SyntaxToken ForKeyword { get; }
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax LowerBound { get; }
    public SyntaxToken ToKeyword { get; }
    public ExpressionSyntax UpperBound { get; }
    public StatementSyntax Body { get; }

    internal ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken identifierToken, SyntaxToken equals,
                                ExpressionSyntax lowerBound, SyntaxToken toKeyWord, ExpressionSyntax upperBound, StatementSyntax body)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ForKeyword = forKeyword ?? throw new ArgumentNullException(nameof(forKeyword));
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
        EqualsToken = equals ?? throw new ArgumentNullException(nameof(equals));
        LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
        ToKeyword = toKeyWord ?? throw new ArgumentNullException(nameof(toKeyWord));
        UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}
