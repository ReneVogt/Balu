using Balu.Expressions;
using System;

namespace Balu;

/// <summary>
/// A parser for the Balu language.
/// </summary>
sealed class Parser : IParser
{
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
        
    }
}
