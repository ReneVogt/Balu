using Balu.Expressions;
using System;
using System.Linq;

namespace Balu;

/// <summary>
/// A parser for the Balu language.
/// </summary>
sealed class Parser : IParser
{
    static readonly NumberExpressionSyntax dummyNumber = new NumberExpressionSyntax(SyntaxToken.Number(0, 0, string.Empty));
    readonly ILexer lexer;


    /// <summary>
    /// Creates a new <see cref="Parser"/> for the given <paramref name="input"/> of Balu code.
    /// </summary>
    /// <param name="input">The input Balu code to parse.</param>
    public Parser(string input) => lexer = new Lexer(input);

    /// <summary>
    /// Creates a new <see cref="Parser"/> using the given <paramref name="lexer"/> implementation.
    /// </summary>
    /// <param name="lexer">An <see cref="ILexer"/> implementation that can build <see cref="SyntaxToken">syntax tokens</see> from a given Balu code.</param>
    public Parser(ILexer lexer) => this.lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));

    /// <inheritdoc/>
    public ExpressionSyntax Parse()
    {
        var tokens = lexer.GetTokens()
                          .Where(token => token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpaceToken &&
                                          token.Kind != SyntaxKind.EndOfFileToken)
                          .ToArray();

        if (tokens.Length == 0) return dummyNumber;
        
        int position = 0;
        var left = ParseNumberExpression();
        while (position < tokens.Length)
        {
            var operatorToken = tokens[position++];
            var right = ParseNumberExpression();
            left = ExpressionSyntax.Binary(left, operatorToken, right);
        }

        return left;

        ExpressionSyntax ParseNumberExpression() =>
            position < tokens.Length && tokens[position].Kind == SyntaxKind.NumberToken ? new NumberExpressionSyntax(tokens[position++]) : dummyNumber;
    }
}
