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

    public bool IsMissing { get; }

    internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, TextSpan span, string? text, object? value,
                         ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia)
        : base(syntaxTree)
    {
        Kind = kind;
        Text = text ?? string.Empty;
        Span = span;
        Value = value;

        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;

        IsMissing = text is null;

        var start = LeadingTrivia.Length == 0 ? Span.Start : LeadingTrivia[0].Span.Start;
        var end = TrailingTrivia.Length == 0 ? Span.End : TrailingTrivia.Last().Span.End;
        FullSpan = new(start, end - start);
    }
    
    public override string ToString() => $"{Kind}{Span} \"{Text}\" ({(Value is string v ? v.EscapeString() : Value?.ToString())})";
}
