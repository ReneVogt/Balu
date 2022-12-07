using Balu.Text;
using System;
using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// A lexer for the Balu language.
/// </summary>
sealed class Lexer
{
    readonly SourceText input;
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
    internal Lexer(SourceText input)
    {
        this.input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>
    /// Enumerates the <see cref="SyntaxToken">syntax tokens</see> from the input Balu code.
    /// </summary>
    /// <returns>A sequence of <see cref="SyntaxToken">syntax tokens</see>.</returns>
    public IEnumerable<SyntaxToken> Lex()
    {
        do
        {
            start = position;
            value = null;
            kind = SyntaxKind.BadToken;

            if (char.IsDigit(Current))
                ReadNumberToken();
            else if (char.IsWhiteSpace(Current))
                ReadWhiteSpaceToken();
            else if (char.IsLetter(Current))
                ReadIdentifierOrKeywordToken();
            else
            {
                kind = (Current, Peek(1)) switch
                {
                    ('\0', _) => SyntaxKind.EndOfFileToken,
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

                text = kind.GetText() ?? Current.ToString();
                position += text.Length;
            }

            if (kind == SyntaxKind.BadToken)
                diagnostics.ReportUnexpectedToken(start, position - start, input[start].ToString());

            yield return new(kind, new(start, kind == SyntaxKind.EndOfFileToken ? 0 :  position - start), text, value);

        } while (kind != SyntaxKind.EndOfFileToken);
    }
    char Peek(int offset)
    {
        var index = position + offset;
        return index >= input.Length ? '\0' : input[index];
    }
    char Current => Peek(0);
    void Next()
    {
        if (position < input.Length) position++;
    }

    void ReadNumberToken()
    {
        kind = SyntaxKind.NumberToken;
        while (char.IsDigit(Current)) Next();
        text = input.ToString(start, position - start);
        if (int.TryParse(text, out var v))
            value = v;
        else
            diagnostics.ReportNumberNotValid(start, position - start, text);
    }
    void ReadWhiteSpaceToken()
    {
        kind = SyntaxKind.WhiteSpaceToken;
        while (char.IsWhiteSpace(Current)) Next();
        text = input.ToString(start, position - start);
    }
    void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current)) Next();
        text = input.ToString(start, position - start);
        kind = text.KeywordKind();
        value = kind switch
        {
            SyntaxKind.TrueKeyword
                => true,
            SyntaxKind.FalseKeyword => false,
            _ => null
        };
    }
}
