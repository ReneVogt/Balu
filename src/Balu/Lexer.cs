using System;
using System.Collections.Generic;

namespace Balu;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer
{
    readonly string input;
    readonly List<string> diagnostics = new();

    /// <summary>
    /// The list of error messages.
    /// </summary>
    public IEnumerable<string> Diagnostics => diagnostics;

    /// <summary>
    /// Creates a new <see cref="Lexer"/> instance.
    /// </summary>
    /// <param name="input">The input Balu code.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    internal Lexer(string input)
    {
        this.input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>
    /// Enumerates the <see cref="SyntaxToken">syntax tokens</see> from the input Balu code.
    /// </summary>
    /// <returns>A sequence of <see cref="SyntaxToken">syntax tokens</see>.</returns>
    public IEnumerable<SyntaxToken> GetTokens()
    {
        diagnostics.Clear();
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

            switch(input[position])
            {
                case '+':
                    yield return SyntaxToken.Plus(position);
                    break;
                case '-':
                    yield return SyntaxToken.Minus(position);
                    break;
                case '*':
                    yield return SyntaxToken.Star(position);
                    break;
                    case '/':
                        yield return SyntaxToken.Slash(position);
                        break;
                        case '(':
                            yield return SyntaxToken.OpenParenthesis(position);
                            break;
                case ')':
                    yield return SyntaxToken.ClosedParenthesis(position);
                    break;
                default:
                    diagnostics.Add($"ERROR: Unexpected token at {position}: '{input[position]}'.");
                    yield return SyntaxToken.Bad(position, input[position].ToString());
                    break;
            }

            position++;

        }

        yield return SyntaxToken.EndOfFile(position);
    }
}
