using Balu.Text;

namespace Balu.Syntax;

public sealed class SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, string text, TextSpan span)
{
    public SyntaxTree SyntaxTree { get; } = syntaxTree;
    public SyntaxKind Kind { get; } = kind;
    public string Text { get; } = text;
    public TextSpan Span { get; } = span;
}
