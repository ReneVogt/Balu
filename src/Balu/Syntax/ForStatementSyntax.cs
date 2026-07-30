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

    internal ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken identifierToken, SyntaxToken equalsToken,
                                ExpressionSyntax lowerBound, SyntaxToken toKeyword, ExpressionSyntax upperBound, StatementSyntax body)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ForKeyword = forKeyword ?? throw new ArgumentNullException(nameof(forKeyword));
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
        EqualsToken = equalsToken ?? throw new ArgumentNullException(nameof(equalsToken));
        LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
        ToKeyword = toKeyword ?? throw new ArgumentNullException(nameof(toKeyword));
        UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}
