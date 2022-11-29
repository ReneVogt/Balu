using System;
using System.Collections.Generic;

namespace Balu;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer : ILexer
{
    readonly string input;

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
        int position = 0;
        while (position < input.Length)
        {
            if (char.IsDigit(input[position]))
            {
                int index = position;
                while (index < input.Length && char.IsDigit(input[index])) index++;
                string text = input[position..index];
                int.TryParse(text, out var value);
                yield return SyntaxToken.Number(value, position, text);
                position = index;
                continue;
            }

            if (char.IsWhiteSpace(input[position]))
            {
                int index = position;
                while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
                string text = input[position..index];
                yield return SyntaxToken.WhiteSpace(position, text);
                position = index;
                continue;
            }

            yield return input[position] switch
            {
                '+' => SyntaxToken.Plus(position),
                '-' => SyntaxToken.Minus(position),
                '*' => SyntaxToken.Star(position),
                '/' => SyntaxToken.Slash(position),
                '(' => SyntaxToken.OpenParenthesis(position),
                ')' => SyntaxToken.ClosedParenthesis(position),
                _ => SyntaxToken.Bad(position, input[position].ToString())
            };

            position++;

        }

        yield return SyntaxToken.EndOfFile(position);
    }
}
