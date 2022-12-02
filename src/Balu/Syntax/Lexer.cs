using System;
using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer
{
    readonly string input;
    readonly List<Diagnostic> diagnostics = new();

    /// <summary>
    /// The list of error messages.
    /// </summary>
    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

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
                    diagnostics.Add(Diagnostic.LexerNumberNotValid(position, index - position, text));
                yield return SyntaxToken.Number(value, new(position, index-position), text);
                position = index;
                continue;
            }

            if (char.IsWhiteSpace(input[position]))
            {
                int index = position;
                while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
                string text = input[position..index];
                yield return SyntaxToken.WhiteSpace(new(position, index - position), text);
                position = index;
                continue;
            }

            if (char.IsLetter(input[position]))
            {
                int index = position;
                while (index < input.Length && char.IsLetter(input[index])) index++;
                string keyword = input[position..index];
                yield return new SyntaxToken(keyword.KeywordKind(), new(position, index - position), keyword);
                position = index;
                continue;
            }

            switch (input[position])
            {
                case '+':
                    yield return SyntaxToken.Plus(new(position++, 1));
                    break;
                case '-':
                    yield return SyntaxToken.Minus(new(position++, 1));
                    break;
                case '*':
                    yield return SyntaxToken.Star(new(position++, 1));
                    break;
                case '/':
                    yield return SyntaxToken.Slash(new(position++, 1));
                    break;
                case '(':
                    yield return SyntaxToken.OpenParenthesis(new(position++, 1));
                    break;
                case ')':
                    yield return SyntaxToken.ClosedParenthesis(new(position++, 1));
                    break;
                case '!':
                    if (Peek(1) == '=')
                    {
                        yield return SyntaxToken.NotEquals(new(position, 2));
                        position += 2;
                    }
                    else
                        yield return SyntaxToken.Bang(new(position++, 1));
                    break;
                case '&':
                    if (Peek(1) != '&') goto default;
                    yield return SyntaxToken.AmpersandAmpersand(new(position, 2));
                    position += 2;
                    break;
                case '|':
                    if (Peek(1) != '|') goto default;
                    yield return SyntaxToken.PipePipe(new(position, 2));
                    position += 2;
                    break;
                case '=':
                    if (Peek(1) != '=') goto default;
                    yield return SyntaxToken.EqualsEquals(new(position, 2));
                    position += 2;
                    break;
                default:
                    diagnostics.Add(Diagnostic.LexerUnexpectedToken(position, 1, input[position].ToString()));
                    yield return SyntaxToken.Bad(new(position, 1), input[position].ToString());
                    position++;
                    break;
            }
        }

        yield return SyntaxToken.EndOfFile(new(position, 0));
        
        char Peek(int offset)
        {
            var index = position + offset;
            return index >= input.Length ? '\0' : input[index];
        }
    }
}
