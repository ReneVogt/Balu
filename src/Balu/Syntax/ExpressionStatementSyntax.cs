using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class ExpressionStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get { yield return Expression; }
    }
    public ExpressionSyntax Expression { get; }

    internal ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax? expression)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));
}
