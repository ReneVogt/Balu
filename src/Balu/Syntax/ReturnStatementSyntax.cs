using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ReturnStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return ReturnKeyword;
            if (Expression is not null) yield return Expression;
        }
    }

    public SyntaxToken ReturnKeyword { get; }
    public ExpressionSyntax? Expression { get; }

    public ReturnStatementSyntax(SyntaxTree? syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword ?? throw new ArgumentNullException(nameof(returnKeyword));
        Expression = expression;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var returnKeyword = (SyntaxToken)visitor.Visit(ReturnKeyword);
        var expression = Expression is null ? null : (ExpressionSyntax)visitor.Visit(Expression);
        return returnKeyword == ReturnKeyword && expression == Expression ? this : new(null, returnKeyword, expression);
    }
}
