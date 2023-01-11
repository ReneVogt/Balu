using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ForStatement;
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
    public SyntaxToken ForKeyword { get; }
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax LowerBound { get; }
    public SyntaxToken ToKeyword { get; }
    public ExpressionSyntax UpperBound { get; }
    public StatementSyntax Body { get; }

    public ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken identifierToken, SyntaxToken equals,
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
    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter)
    {
        var forKeyword = (SyntaxToken)rewriter.Visit(ForKeyword);
        var identifier = (SyntaxToken)rewriter.Visit(IdentifierToken);
        var equals = (SyntaxToken)rewriter.Visit(EqualsToken);
        var lowerBound = (ExpressionSyntax)rewriter.Visit(LowerBound);
        var toKeyWord = (SyntaxToken)rewriter.Visit(ToKeyword);
        var upperBound = (ExpressionSyntax)rewriter.Visit(UpperBound);
        var body = (StatementSyntax)rewriter.Visit(Body);
        return forKeyword == ForKeyword &&
               identifier == IdentifierToken &&
               equals == EqualsToken &&
               lowerBound == LowerBound &&
               toKeyWord == ToKeyword &&
               upperBound == UpperBound &&
               body == Body
                   ? this
                   : new(SyntaxTree, forKeyword, identifier, equals, lowerBound, toKeyWord, upperBound, body);
    }
}
