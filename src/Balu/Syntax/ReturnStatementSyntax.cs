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

    public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        ReturnKeyword = returnKeyword ?? throw new ArgumentNullException(nameof(returnKeyword));
        Expression = expression;
    }
}
