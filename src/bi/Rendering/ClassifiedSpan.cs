using Balu.Text;

namespace Balu.Interactive.Rendering;

sealed class ClassifiedSpan
{
    public TextSpan Span { get; }
    public Classification Classification { get; }

    public ClassifiedSpan(TextSpan span, Classification classification)
    {
        Span = span;
        Classification = classification;
    }
}
