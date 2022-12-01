using System;
using System.Collections.Generic;

namespace Balu.Syntax;

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
    public IEnumerable<SyntaxToken> Lex()
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
                if (!int.TryParse(text, out var value))
                    diagnostics.Add($"ERROR: The number '{text}' at position {position} is not a valid 32bit integer.");
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

            if (char.IsLetter(input[position]))
            {
                int index = position;
                while (index < input.Length && char.IsLetter(input[index])) index++;
                string keyword = input[position..index];
                yield return new SyntaxToken(keyword.KeywordKind(), position, keyword);
                position = index;
                continue;
            }

            switch (input[position])
            {
                case '+':
                    yield return SyntaxToken.Plus(position++);
                    break;
                case '-':
                    yield return SyntaxToken.Minus(position++);
                    break;
                case '*':
                    yield return SyntaxToken.Star(position++);
                    break;
                case '/':
                    yield return SyntaxToken.Slash(position++);
                    break;
                case '(':
                    yield return SyntaxToken.OpenParenthesis(position++);
                    break;
                case ')':
                    yield return SyntaxToken.ClosedParenthesis(position++);
                    break;
                case '!':
                    yield return SyntaxToken.Bang(position++);
                    break;
                case '&':
                    if (Peek(1) != '&') goto default;
                    yield return SyntaxToken.AmpersandAmpersand(position);
                    position += 2;
                    break;
                case '|':
                    if (Peek(1) != '|') goto default;
                    yield return SyntaxToken.PipePipe(position);
                    position += 2;
                    break;
                default:
                    diagnostics.Add($"ERROR: Unexpected token at {position}: '{input[position]}'.");
                    yield return SyntaxToken.Bad(position, input[position].ToString());
                    position++;
                    break;
            }
        }

        yield return SyntaxToken.EndOfFile(position);
        
        char Peek(int offset)
        {
            var index = position + offset;
            return index >= input.Length ? '\0' : input[index];
        }
    }
}
