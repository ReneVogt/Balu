using System;

namespace Balu.Syntax;

public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken LiteralToken { get; }
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

    public object? Value { get; }

    internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken) : this(syntaxTree, literalToken ?? throw new ArgumentNullException(nameof(literalToken)), literalToken.Value){}
    internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken, object? value) : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        LiteralToken = literalToken ?? throw new ArgumentNullException(nameof(literalToken));
        Value = value;
    }
}
