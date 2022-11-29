using System;
using System.Collections.Generic;

namespace Balu;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer : ILexer
{
    string input;
    int position;

    /// <summary>
    /// Creates a new <see cref="Lexer"/> instance.
    /// </summary>
    /// <param name="input">The input Balu code.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public Lexer(string input)
    {
        this.input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <inheritdoc/>
    public IEnumerable<SyntaxToken> GetTokens()
    {
        while (position < input.Length)
        {
            position++;
        }

        yield return new SyntaxToken(SyntaxKind.EndOfFileToken, position);
    }
}
