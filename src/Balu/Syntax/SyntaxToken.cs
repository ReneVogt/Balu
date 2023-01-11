using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public override TextSpan FullSpan { get; }
    public object? Value { get; }

    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }
    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    public bool IsMissing => string.IsNullOrEmpty(Text);

    internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, TextSpan span, string text, object? value,
                         ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia)
        : base(syntaxTree)
    {
        Kind = kind;
        Text = text;
        Span = span;
        Value = value;

        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;

        var start = LeadingTrivia.Length == 0 ? Span.Start : LeadingTrivia[0].Span.Start;
        var end = TrailingTrivia.Length == 0 ? Span.End : TrailingTrivia.Last().Span.End;
        FullSpan = new(start, end - start);
    }
    
    internal override SyntaxNode Rewrite(SyntaxTreeRewriter rewriter) => this;

    public override string ToString() => $"{Kind}{Span} \"{Text}\" ({(Value is string v ? v.EscapeString() : Value?.ToString())})";

    public static SyntaxToken EndOfFile(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.EndOfFileToken, span, "\0", null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Number(SyntaxTree syntaxTree, TextSpan span, int value, string text) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.NumberToken, span, text ?? throw new ArgumentNullException(nameof(text)), value, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken String(SyntaxTree syntaxTree, TextSpan span, string value, string text) => new(
        syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.StringToken, span,
        text ?? throw new ArgumentNullException(nameof(text)),
        value ?? throw new ArgumentNullException(nameof(value)), ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken SingleLineComment(SyntaxTree syntaxTree, TextSpan span, string text) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.SingleLineCommentTrivia, span,
        text ?? throw new ArgumentNullException(nameof(text)), null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken MultiLineComment(SyntaxTree syntaxTree, TextSpan span, string text) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.MultiLineCommentTrivia, span,
        text ?? throw new ArgumentNullException(nameof(text)), null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);

    public static SyntaxToken Plus(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.PlusToken, span, SyntaxKind.PlusToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Minus(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.MinusToken, span, SyntaxKind.MinusToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Star(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.StarToken, span, SyntaxKind.StarToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Slash(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.SlashToken, span, SyntaxKind.SlashToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken OpenParenthesis(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.OpenParenthesisToken, span, SyntaxKind.OpenParenthesisToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ClosedParenthesis(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ClosedParenthesisToken, span, SyntaxKind.ClosedParenthesisToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken OpenBrace(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.OpenBraceToken, span, SyntaxKind.OpenBraceToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ClosedBrace(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ClosedBraceToken, span, SyntaxKind.ClosedBraceToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Equals(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.EqualsToken, span, SyntaxKind.EqualsToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Bang(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.BangToken, span, SyntaxKind.BangToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Ampersand(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.AmpersandToken, span, SyntaxKind.AmpersandToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken AmpersandAmpersand(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.AmpersandAmpersandToken, span, SyntaxKind.AmpersandAmpersandToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken PipePipe(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.PipePipeToken, span, SyntaxKind.PipePipeToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Pipe(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.PipeToken, span, SyntaxKind.PipeToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Circumflex(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.CircumflexToken, span, SyntaxKind.CircumflexToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Tilde(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.TildeToken, span, SyntaxKind.TildeToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken EqualsEquals(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.EqualsEqualsToken, span, SyntaxKind.EqualsEqualsToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken BangEquals(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.BangEqualsToken, span, SyntaxKind.BangEqualsToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Less(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.LessToken, span, SyntaxKind.LessToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken LessOrEquals(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.LessOrEqualsToken, span, SyntaxKind.LessOrEqualsToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Greater(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.GreaterToken, span, SyntaxKind.GreaterToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken GreaterOrEquals(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.GreaterOrEqualsToken, span, SyntaxKind.GreaterOrEqualsToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Identifier(SyntaxTree syntaxTree, TextSpan span, string name) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.IdentifierToken, span, name ?? throw new ArgumentNullException(nameof(name)), null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Comma(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.CommaToken, span, SyntaxKind.CommaToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken Colon(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ColonToken, span, SyntaxKind.ColonToken.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken TrueKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.TrueKeyword, span, SyntaxKind.TrueKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken FalseKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.FalseKeyword, span, SyntaxKind.FalseKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken LetKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.LetKeyword, span, SyntaxKind.LetKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken VarKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.VarKeyword, span, SyntaxKind.VarKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken IfKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.IfKeyword, span, SyntaxKind.IfKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ElseKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ElseKeyword, span, SyntaxKind.ElseKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken WhileKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.WhileKeyword, span, SyntaxKind.WhileKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken DoKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.DoKeyword, span, SyntaxKind.DoKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ForKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ForKeyword, span, SyntaxKind.ForKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ToKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ToKeyword, span, SyntaxKind.ToKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ContinueKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ContinueKeyword, span, SyntaxKind.ContinueKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken BreakKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.BreakKeyword, span, SyntaxKind.BreakKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken FunctionKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.FunctionKeyword, span, SyntaxKind.FunctionKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    public static SyntaxToken ReturnKeyword(SyntaxTree syntaxTree, TextSpan span) => new(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)), SyntaxKind.ReturnKeyword, span, SyntaxKind.ReturnKeyword.GetText()!, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
}
