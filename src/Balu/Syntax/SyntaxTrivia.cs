using Balu.Text;

namespace Balu.Syntax;

public sealed class SyntaxTrivia
{
    public SyntaxTree SyntaxTree { get;  }
    public SyntaxKind Kind { get; }
    public string Text { get; }
    public TextSpan Span { get; }

    public SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, string text, TextSpan span)
    {
        SyntaxTree = syntaxTree;
        Kind = kind;
        Text = text;
        Span = span;
    }
}
