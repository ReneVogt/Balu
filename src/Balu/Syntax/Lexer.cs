using Balu.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Balu.Diagnostics;

namespace Balu.Syntax;

sealed class Lexer
{
    readonly SyntaxTree syntaxTree;
    readonly SourceText sourceText;
    readonly CancellationToken cancellationToken;
    readonly DiagnosticBag diagnostics = [];
    readonly ImmutableArray<SyntaxTrivia>.Builder triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    int position, start;
    string text = string.Empty;
    SyntaxKind kind;
    object? value;

    internal Lexer(SyntaxTree syntaxTree, CancellationToken cancellationToken = default)
    {
        this.syntaxTree = syntaxTree;
        this.cancellationToken = cancellationToken;
        sourceText = this.syntaxTree.Text;
    }

    public IEnumerable<SyntaxToken> Lex()
    {
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
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

            yield return new(syntaxTree, tokenKind, new(tokenStart, tokenKind == SyntaxKind.EndOfFileToken ? 0 : tokenLength), tokenText, tokenValue, leadingTrivia, trailingTrivia);

            kind = tokenKind;

        } while (kind != SyntaxKind.EndOfFileToken);
    }

    void ReadTrivia(bool leading)
    {
        triviaBuilder.Clear();

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            start = position;
            kind = CurrentKind();
            text = string.Empty;

            switch (kind)
            {
                case SyntaxKind.WhiteSpaceTrivia:
                    ReadWhiteSpaces();
                    break;
                case SyntaxKind.LineBreakTrivia:
                    ReadLineBreak();
                    break;
                case SyntaxKind.SingleLineCommentTrivia:
                    ReadSingleLineComment();
                    break;
                case SyntaxKind.MultiLineCommentTrivia:
                    ReadMultiLineComment();
                    break;
                default:
                    return;
            }

            triviaBuilder.Add(new(syntaxTree, kind, text, new(start, position - start)));
        } while (leading || sourceText.GetLineIndex(start) == sourceText.GetLineIndex(position));
    }
    void ReadToken()
    {
        start = position;
        value = null;
        kind = CurrentKind();
        text = string.Empty;

        if (kind == SyntaxKind.NumberToken)
            ReadNumberToken();
        else if (kind == SyntaxKind.IdentifierToken)
            ReadIdentifierOrKeywordToken();
        else if (kind == SyntaxKind.StringToken)
            ReadString();
        else if (kind == SyntaxKind.BadToken)
            ReadBadToken();
        else
        {
            text = kind.GetText() ?? Current.ToString();
            if (!IsAtEnd) position += text.Length;
        }
    }
    SyntaxKind CurrentKind()
    {
        if (IsAtEnd)
            return SyntaxKind.EndOfFileToken;
        if (sourceText.GetLineBreakWidth(position) > 0)
            return SyntaxKind.LineBreakTrivia;
        if (char.IsWhiteSpace(Current))
            return SyntaxKind.WhiteSpaceTrivia;
        if (char.IsDigit(Current))
            return SyntaxKind.NumberToken;
        if (char.IsLetter(Current) || Current == '_')
            return SyntaxKind.IdentifierToken;
        if (Current == '"') return SyntaxKind.StringToken;

        return (Current, Peek(1)) switch
        {
            ('+', '=') => SyntaxKind.PlusEqualsToken,
            ('+', '+') => SyntaxKind.PlusPlusToken,
            ('+', _) => SyntaxKind.PlusToken,
            ('-', '=') => SyntaxKind.MinusEqualsToken,
            ('-', '-') => SyntaxKind.MinusMinusToken,
            ('-', _) => SyntaxKind.MinusToken,
            ('*', '=') => SyntaxKind.StarEqualsToken,
            ('*', _) => SyntaxKind.StarToken,
            ('/', '/') => SyntaxKind.SingleLineCommentTrivia,
            ('/', '*') => SyntaxKind.MultiLineCommentTrivia,
            ('/', '=') => SyntaxKind.SlashEqualsToken,
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
            ('&', '=') => SyntaxKind.AmpersandEqualsToken,
            ('&', _) => SyntaxKind.AmpersandToken,
            ('|', '|') => SyntaxKind.PipePipeToken,
            ('|', '=') => SyntaxKind.PipeEqualsToken,
            ('|', _) => SyntaxKind.PipeToken,
            ('^', '=') => SyntaxKind.CircumflexEqualsToken,
            ('^', _) => SyntaxKind.CircumflexToken,
            ('~', _) => SyntaxKind.TildeToken,
            ('>', '=') => SyntaxKind.GreaterOrEqualsToken,
            ('>', _) => SyntaxKind.GreaterToken,
            ('<', '=') => SyntaxKind.LessOrEqualsToken,
            ('<', _) => SyntaxKind.LessToken,
            (',', _) => SyntaxKind.CommaToken,
            (':', _) => SyntaxKind.ColonToken,
            _ => SyntaxKind.BadToken
        };
    }

    char Peek(int offset)
    {
        var index = position + offset;
        return index >= sourceText.Length ? '\0' : sourceText[index];
    }
    char Current => Peek(0);
    bool IsAtEnd => position >= sourceText.Length;
    void Next()
    {
        if (position < sourceText.Length) position++;
    }

    void ReadNumberToken()
    {
        kind = SyntaxKind.NumberToken;
        while (char.IsDigit(Current))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }
        text = sourceText.ToString(start, position - start);
        if (int.TryParse(text, out var v))
            value = v;
        else
            diagnostics.ReportNumberNotValid(new(sourceText, new(start, position - start)), text);
    }
    void ReadWhiteSpaces()
    {
        kind = SyntaxKind.WhiteSpaceTrivia;
        while (!IsAtEnd && sourceText.GetLineBreakWidth(position) == 0 && char.IsWhiteSpace(Current))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }
        text = sourceText.ToString(start, position - start);
    }
    void ReadLineBreak()
    {
        kind = SyntaxKind.LineBreakTrivia;
        position += sourceText.GetLineBreakWidth(position);
        text = sourceText.ToString(start, position - start);
    }
    void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current) || char.IsDigit(Current) || Current == '_')
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }
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
        while (!IsAtEnd && Current != '"')
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (sourceText.GetLineBreakWidth(position) > 0)
            {
                diagnostics.ReportUnterminatedString(new(sourceText, new(start, position - start)));
                text = sourceText.ToString(start, position - start);
                kind = SyntaxKind.StringToken;
                return;
            }

            switch (Current)
            {
                case '\\':
                    if (position + 1 >= sourceText.Length)
                    {
                        diagnostics.ReportInvalidEscapeSequence(new(sourceText, new(position, 1)), "\\");
                        break;
                    }

                    char next = Peek(1);
                    if (SyntaxFacts.EscapedToUnescapedCharacter.TryGetValue(next.ToString(), out var unescaped))
                    {
                        valueBuilder.Append(unescaped);
                        Next();
                    }
                    else
                        diagnostics.ReportInvalidEscapeSequence(new(sourceText, new(position + 1, 1)), next.ToString());
                    break;
                default:
                    valueBuilder.Append(Current);
                    break;
            }

            Next();
        }

        if (IsAtEnd)
        {
            diagnostics.ReportUnterminatedString(new(sourceText, new(start, position - start)));
            text = sourceText.ToString(start, position - start);
            kind = SyntaxKind.StringToken;
            return;
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
        while (!IsAtEnd && sourceText.GetLineBreakWidth(position) == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }
        position += sourceText.GetLineBreakWidth(position);
        text = sourceText.ToString(start, position - start);
    }
    void ReadMultiLineComment()
    {
        kind = SyntaxKind.MultiLineCommentTrivia;
        value = null;
        position += 2;
        while (!IsAtEnd && (Current != '*' || Peek(1) != '/'))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }

        if (IsAtEnd)
            diagnostics.ReportUnterminatedMultiLineComment(new(sourceText, new(start, 2)));
        else
            position += 2;
        text = sourceText.ToString(start, position - start);
    }
    void ReadBadToken()
    {
        kind = SyntaxKind.BadToken;
        value = null;
        while (CurrentKind() == SyntaxKind.BadToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Next();
        }
        text = sourceText.ToString(start, position - start);
        diagnostics.ReportUnexpectedToken(new(sourceText, new(start, position - start)));
    }
}
