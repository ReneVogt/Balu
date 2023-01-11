﻿using System;
using Balu.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Balu.Syntax;

sealed class Lexer
{
    readonly SyntaxTree syntaxTree;
    readonly SourceText sourceText;
    readonly DiagnosticBag diagnostics = new();
    readonly ImmutableArray<SyntaxTrivia>.Builder triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

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
            ReadTrivia(true);
            var leadingTrivia = triviaBuilder.ToImmutable();

            var tokenStart = position;
            ReadToken();
            var tokenKind = kind;
            var tokenValue = value;
            var tokenLength = position - tokenStart;
            var tokenText = text;
            
            ReadTrivia(false);
            var trailingTrivia = triviaBuilder.ToImmutable();

            yield return new(syntaxTree, tokenKind, new(tokenStart, tokenKind == SyntaxKind.EndOfFileToken ? 0 :  tokenLength), tokenText, tokenValue, leadingTrivia, trailingTrivia);

            kind = tokenKind;

        } while (kind != SyntaxKind.EndOfFileToken);
    }

    void ReadTrivia(bool leading)
    {
        triviaBuilder.Clear();

        do
        {
            start = position;
            kind = CurrentKind();

            switch (kind)
            {
                case SyntaxKind.WhiteSpaceTrivia:
                    ReadWhiteSpaces();
                    break;
                case SyntaxKind.SingleLineCommentTrivia:
                    ReadSingleLineComment();
                    break;
                case SyntaxKind.MultiLineCommentTrivia:
                    ReadMultiLineComment();
                    break;
                case SyntaxKind.BadTokenTrivia:
                    ReadBadTokenTrivia();
                    break;
                default:
                    return;
            }

            triviaBuilder.Add(new(syntaxTree, kind, text, new(start, position - start)));
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        } while (leading || !text.Contains('\n', StringComparison.InvariantCulture));
    }
    void ReadToken()
    {
        start = position;
        value = null; 
        kind = CurrentKind();

        if (kind == SyntaxKind.NumberToken)
            ReadNumberToken();
        else if (kind == SyntaxKind.IdentifierToken)
            ReadIdentifierOrKeywordToken();
        else if (kind == SyntaxKind.StringToken)
            ReadString();
        else
        {
            text = kind.GetText() ?? Current.ToString();
            if (Current != '\0') position += text.Length;
        }
    }
    SyntaxKind CurrentKind()
    {
        if (Current == '\0') 
            return SyntaxKind.EndOfFileToken;
        if (char.IsWhiteSpace(Current))
            return SyntaxKind.WhiteSpaceTrivia;
        if (char.IsDigit(Current))
            return SyntaxKind.NumberToken;
        if (char.IsLetter(Current) || Current == '_')
            return SyntaxKind.IdentifierToken;
        if (Current == '"') return SyntaxKind.StringToken;

        return (Current, Peek(1)) switch
        {
            ('\0', _) => SyntaxKind.EndOfFileToken,
            ('+', _) => SyntaxKind.PlusToken,
            ('-', _) => SyntaxKind.MinusToken,
            ('*', _) => SyntaxKind.StarToken,
            ('/', '/') => SyntaxKind.SingleLineCommentTrivia,
            ('/', '*') => SyntaxKind.MultiLineCommentTrivia,
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
            (',', _) => SyntaxKind.CommaToken,
            (':', _) => SyntaxKind.ColonToken,
            _ => SyntaxKind.BadTokenTrivia
        };
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
            diagnostics.ReportNumberNotValid(new(sourceText, new(start, position - start)), text);
    }
    void ReadWhiteSpaces()
    {
        kind = SyntaxKind.WhiteSpaceTrivia;
        while (char.IsWhiteSpace(Current)) Next();
        text = sourceText.ToString(start, position - start);
    }
    void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current) || char.IsDigit(Current) || Current == '_') Next();
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
                    diagnostics.ReportUnterminatedString(new (sourceText, new(start, position - start)));
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
                        diagnostics.ReportInvalidEscapeSequence(new(sourceText, new(position+1, 1)), next.ToString());
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
    void ReadSingleLineComment()
    {
        value = null;
        kind = SyntaxKind.SingleLineCommentTrivia;
        while (Current != '\0' && Current != '\n') Next();
        if (Current != '\0') Next();
        text = sourceText.ToString(start, position - start);
    }
    void ReadMultiLineComment()
    {
        kind = SyntaxKind.MultiLineCommentTrivia;
        value = null;
        char c1, c2;
        position++;
        do
        {
            Next();
            c1 = Current;
            c2 = Peek(1);
        } while (c2 != '\0' && (c1 != '*' || c2 != '/'));

        if (c2 == '\0')
            diagnostics.ReportUnterminatedMultiLineComment(new(sourceText, new (start, 2)));
        Next();
        Next();
        text = sourceText.ToString(start, position - start);
    }
    void ReadBadTokenTrivia()
    {
        kind = SyntaxKind.BadTokenTrivia;
        value = null;
        while (CurrentKind() == SyntaxKind.BadTokenTrivia) Next();
        text = sourceText.ToString(start, position - start);
        diagnostics.ReportUnexpectedToken(new(sourceText, new(start, position - start)));
    }
}
