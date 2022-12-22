﻿using Balu.Text;
using System.Collections.Generic;
using System.Text;

namespace Balu.Syntax;

sealed class Lexer
{
    readonly SyntaxTree syntaxTree;
    readonly SourceText sourceText;
    readonly DiagnosticBag diagnostics = new();

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    int position, start;
    string text = string.Empty;
    SyntaxKind kind;
    object? value;

    internal Lexer(SyntaxTree syntaxTree)
    {
        this.syntaxTree = syntaxTree;
        sourceText = this.syntaxTree.Text;
    }

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
            else if (Current == '"')
                ReadString();
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
                    ('{', _) => SyntaxKind.OpenBraceToken,
                    ('}', _) => SyntaxKind.ClosedBraceToken,
                    ('!', '=') => SyntaxKind.BangEqualsToken,
                    ('!', _) => SyntaxKind.BangToken,
                    ('=', '=') => SyntaxKind.EqualsEqualsToken,
                    ('=', _) => SyntaxKind.EqualsToken,
                    ('&', '&') => SyntaxKind.AmpersandAmpersandToken,
                    ('&', _) => SyntaxKind.AmpersandToken,
                    ('|', '|') => SyntaxKind.PipePipeToken,
                    ('|', _) => SyntaxKind.PipeToken,
                    ('^', _) => SyntaxKind.CircumflexToken,
                    ('~', _) => SyntaxKind.TildeToken,
                    ('>', '=') => SyntaxKind.GreaterOrEqualsToken,
                    ('>', _) => SyntaxKind.GreaterToken,
                    ('<', '=') => SyntaxKind.LessOrEqualsToken,
                    ('<', _) => SyntaxKind.LessToken,
                    (',',_) => SyntaxKind.CommaToken,
                    (':',_) => SyntaxKind.ColonToken,
                    _ => SyntaxKind.BadToken
                };

                text = kind.GetText() ?? Current.ToString();
                position += text.Length;
            }

            if (kind == SyntaxKind.BadToken)
                diagnostics.ReportUnexpectedToken(start, position - start, sourceText[start].ToString());

            yield return new(kind, new(start, kind == SyntaxKind.EndOfFileToken ? 0 :  position - start), text, value);

        } while (kind != SyntaxKind.EndOfFileToken);
    }
    char Peek(int offset)
    {
        var index = position + offset;
        return index >= sourceText.Length ? '\0' : sourceText[index];
    }
    char Current => Peek(0);
    void Next()
    {
        if (position < sourceText.Length) position++;
    }

    void ReadNumberToken()
    {
        kind = SyntaxKind.NumberToken;
        while (char.IsDigit(Current)) Next();
        text = sourceText.ToString(start, position - start);
        if (int.TryParse(text, out var v))
            value = v;
        else
            diagnostics.ReportNumberNotValid(start, position - start, text);
    }
    void ReadWhiteSpaceToken()
    {
        kind = SyntaxKind.WhiteSpaceToken;
        while (char.IsWhiteSpace(Current)) Next();
        text = sourceText.ToString(start, position - start);
    }
    void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current)) Next();
        text = sourceText.ToString(start, position - start);
        kind = text.KeywordKind();
        value = kind switch
        {
            SyntaxKind.TrueKeyword
                => true,
            SyntaxKind.FalseKeyword => false,
            _ => null
        };
    }

    void ReadString()
    {
        position++; 
        var valueBuilder = new StringBuilder();
        while (Current != '"')
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    diagnostics.ReportUnterminatedString(start, position - start);
                    text = sourceText.ToString(start, position - start);
                    kind = SyntaxKind.StringToken;
                    return;
                case '\\':
                    char next = Peek(1);
                    if (SyntaxFacts.EscapedToUnescapedCharacter.TryGetValue(next.ToString(), out var unescaped))
                    {
                        valueBuilder.Append(unescaped);
                        Next();
                    }
                    else
                        diagnostics.ReportInvalidEscapeSequence(position+1, 1, next.ToString());
                    break;
                default:
                    valueBuilder.Append(Current);
                    break;
            }

            Next();
        }
        Next(); // skip closing "
        text = sourceText.ToString(start, position - start);
        kind = SyntaxKind.StringToken;
        value = valueBuilder.ToString();
    }
}
