using System;
using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer
{
    readonly string input;
    readonly DiagnosticBag diagnostics = new();

    /// <summary>
    /// The list of error messages.
    /// </summary>
    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    int position, start;
    string text = string.Empty;
    SyntaxKind kind;
    object? value;

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
        while (position < input.Length)
        {
            start = position;
            value = null;
            kind = SyntaxKind.BadToken;

            if (char.IsDigit(input[position]))
                ReadNumberToken();
            else if (char.IsWhiteSpace(input[position]))
                ReadWhiteSpaceToken();
            else if (char.IsLetter(input[position]))
                ReadIdentifierOrKeywordToken();
            else
            {
                kind = (input[position], Peek(1)) switch
                {
                    ('+', _) => SyntaxKind.PlusToken,
                    ('-', _) => SyntaxKind.MinusToken,
                    ('*', _) => SyntaxKind.StarToken,
                    ('/', _) => SyntaxKind.SlashToken,
                    ('(', _) => SyntaxKind.OpenParenthesisToken,
                    (')', _) => SyntaxKind.ClosedParenthesisToken,
                    ('!', '=') => SyntaxKind.BangEqualsToken,
                    ('!', _) => SyntaxKind.BangToken,
                    ('=', '=') => SyntaxKind.EqualsEqualsToken,
                    ('=', _) => SyntaxKind.EqualsToken,
                    ('&', '&') => SyntaxKind.AmpersandAmpersandToken,
                    ('|', '|') => SyntaxKind.PipePipeToken,
                    _ => SyntaxKind.BadToken
                };

                text = kind.GetText() ?? input[position].ToString();
                position += text.Length;
            }

            if (kind == SyntaxKind.BadToken)
                diagnostics.ReportUnexpectedToken(start, position-start, input[position].ToString());

            yield return new (kind, new(start, position-start), text, value);
        }

        yield return SyntaxToken.EndOfFile(new(position, 0));
        
        char Peek(int offset)
        {
            var index = position + offset;
            return index >= input.Length ? '\0' : input[index];
        }
    }
    void ReadNumberToken()
    {
        kind = SyntaxKind.NumberToken;
        while (position < input.Length && char.IsDigit(input[position])) position++;
        text = input[start..position];
        if (int.TryParse(text, out var v))
            value = v;
        else
            diagnostics.ReportNumberNotValid(start, position - start, text);
    }
    void ReadWhiteSpaceToken()
    {
        kind = SyntaxKind.WhiteSpaceToken;
        while (position < input.Length && char.IsWhiteSpace(input[position])) position++;
        text = input[start..position];
    }
    void ReadIdentifierOrKeywordToken()
    {
        while (position < input.Length && char.IsLetter(input[position])) position++;
        text = input[start..position];
        kind = text.KeywordKind();
    }
}
