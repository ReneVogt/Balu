namespace Balu.Text;

public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;
    public override string ToString() => $"({Start}, {Length})";
    public bool OverlapsWith(TextSpan other) => Start < other.End && other.Start < End;
    public bool OverlapsWithOrTouches(TextSpan other) => Start <= other.End && other.Start <= End;
}
