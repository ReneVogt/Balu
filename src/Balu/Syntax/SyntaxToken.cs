using System;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

#pragma warning disable CA1720 // "Identifier contains type name" -> STring() factory method for string tokens

public sealed class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }
    public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();
    public string Text { get; }
    public override TextSpan Span { get; }
    public object? Value { get; }

    public bool IsMissing => string.IsNullOrEmpty(Text);

    internal SyntaxToken(SyntaxTree? syntaxTree, SyntaxKind kind, TextSpan span = default, string text = "", object? value = null)
        : base(syntaxTree)
    {
        Kind = kind;
        Text = text;
        Span = span;
        Value = value;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor) => this;

    public override string ToString() => $"{Kind}{Span} \"{Text}\" ({(Value is string v ? v.EscapeString() : Value?.ToString())})";

    public static SyntaxToken EndOfFile(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.EndOfFileToken, span);
    public static SyntaxToken Bad(SyntaxTree? syntaxTree, TextSpan span, string text) => new(syntaxTree, SyntaxKind.BadToken, span, text);
    public static SyntaxToken WhiteSpace(SyntaxTree? syntaxTree, TextSpan span, string text) => new(syntaxTree, SyntaxKind.WhiteSpaceToken, span, text ?? throw new ArgumentNullException(nameof(text)));
    public static SyntaxToken Number(SyntaxTree? syntaxTree, TextSpan span, int value, string text) => new(syntaxTree, SyntaxKind.NumberToken, span, text ?? throw new ArgumentNullException(nameof(text)), value);
    public static SyntaxToken String(SyntaxTree? syntaxTree, TextSpan span, string value, string text) => new(syntaxTree, SyntaxKind.StringToken, span,
                                                                                      text ?? throw new ArgumentNullException(nameof(text)),
                                                                                      value ?? throw new ArgumentNullException(nameof(value)));
    public static SyntaxToken Plus(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.PlusToken, span, SyntaxKind.PlusToken.GetText()!);
    public static SyntaxToken Minus(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.MinusToken, span, SyntaxKind.MinusToken.GetText()!);
    public static SyntaxToken Star(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.StarToken, span, SyntaxKind.StarToken.GetText()!);
    public static SyntaxToken Slash(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.SlashToken, span, SyntaxKind.SlashToken.GetText()!);
    public static SyntaxToken OpenParenthesis(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.OpenParenthesisToken, span, SyntaxKind.OpenParenthesisToken.GetText()!);
    public static SyntaxToken ClosedParenthesis(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ClosedParenthesisToken, span, SyntaxKind.ClosedParenthesisToken.GetText()!);
    public static SyntaxToken OpenBrace(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.OpenBraceToken, span, SyntaxKind.OpenBraceToken.GetText()!);
    public static SyntaxToken ClosedBrace(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ClosedBraceToken, span, SyntaxKind.ClosedBraceToken.GetText()!);
    public static SyntaxToken Equals(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.EqualsToken, span, SyntaxKind.EqualsToken.GetText()!);
    public static SyntaxToken Bang(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.BangToken, span, SyntaxKind.BangToken.GetText()!);
    public static SyntaxToken Ampersand(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.AmpersandToken, span, SyntaxKind.AmpersandToken.GetText()!);
    public static SyntaxToken AmpersandAmpersand(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.AmpersandAmpersandToken, span, SyntaxKind.AmpersandAmpersandToken.GetText()!);
    public static SyntaxToken PipePipe(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.PipePipeToken, span, SyntaxKind.PipePipeToken.GetText()!);
    public static SyntaxToken Pipe(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.PipeToken, span, SyntaxKind.PipeToken.GetText()!);
    public static SyntaxToken Circumflex(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.CircumflexToken, span, SyntaxKind.CircumflexToken.GetText()!);
    public static SyntaxToken Tilde(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.TildeToken, span, SyntaxKind.TildeToken.GetText()!);
    public static SyntaxToken EqualsEquals(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.EqualsEqualsToken, span, SyntaxKind.EqualsEqualsToken.GetText()!);
    public static SyntaxToken BangEquals(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.BangEqualsToken, span, SyntaxKind.BangEqualsToken.GetText()!);
    public static SyntaxToken Less(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.LessToken, span, SyntaxKind.LessToken.GetText()!);
    public static SyntaxToken LessOrEquals(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.LessOrEqualsToken, span, SyntaxKind.LessOrEqualsToken.GetText()!);
    public static SyntaxToken Greater(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.GreaterToken, span, SyntaxKind.GreaterToken.GetText()!);
    public static SyntaxToken GreaterOrEquals(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.GreaterOrEqualsToken, span, SyntaxKind.GreaterOrEqualsToken.GetText()!);
    public static SyntaxToken Identifier(SyntaxTree? syntaxTree, TextSpan span, string name) => new(syntaxTree, SyntaxKind.IdentifierToken, span, name ?? throw new ArgumentNullException(nameof(name)));
    public static SyntaxToken Comma(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.CommaToken, span, SyntaxKind.CommaToken.GetText()!);
    public static SyntaxToken Colon(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ColonToken, span, SyntaxKind.ColonToken.GetText()!);
    public static SyntaxToken TrueKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.TrueKeyword, span, SyntaxKind.TrueKeyword.GetText()!);
    public static SyntaxToken FalseKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.FalseKeyword, span, SyntaxKind.FalseKeyword.GetText()!);
    public static SyntaxToken LetKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.LetKeyword, span, SyntaxKind.LetKeyword.GetText()!);
    public static SyntaxToken VarKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.VarKeyword, span, SyntaxKind.VarKeyword.GetText()!);
    public static SyntaxToken IfKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.IfKeyword, span, SyntaxKind.IfKeyword.GetText()!);
    public static SyntaxToken ElseKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ElseKeyword, span, SyntaxKind.ElseKeyword.GetText()!);
    public static SyntaxToken WhileKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.WhileKeyword, span, SyntaxKind.WhileKeyword.GetText()!);
    public static SyntaxToken DoKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.DoKeyword, span, SyntaxKind.DoKeyword.GetText()!);
    public static SyntaxToken ForKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ForKeyword, span, SyntaxKind.ForKeyword.GetText()!);
    public static SyntaxToken ToKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ToKeyword, span, SyntaxKind.ToKeyword.GetText()!);
    public static SyntaxToken ContinueKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ContinueKeyword, span, SyntaxKind.ContinueKeyword.GetText()!);
    public static SyntaxToken BreakKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.BreakKeyword, span, SyntaxKind.BreakKeyword.GetText()!);
    public static SyntaxToken FunctionKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.FunctionKeyword, span, SyntaxKind.FunctionKeyword.GetText()!);
    public static SyntaxToken ReturnKeyword(SyntaxTree? syntaxTree, TextSpan span) => new(syntaxTree, SyntaxKind.ReturnKeyword, span, SyntaxKind.ReturnKeyword.GetText()!);
}
