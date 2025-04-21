using Balu.Text;

namespace Balu.Authoring;

public sealed class ClassifiedSpan(TextSpan span, Classification classification)
{
    public TextSpan Span { get; } = span;
    public Classification Classification { get; } = classification;
}
