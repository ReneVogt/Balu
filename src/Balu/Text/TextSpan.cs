using System;

namespace Balu.Text;

public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;
    public override string ToString() => $"({Start}, {Length})";
    public bool OverlapsWith(TextSpan other) => Start < other.End && other.Start < End;
    public bool OverlapsWithOrTouches(TextSpan other) => Start <= other.End && other.Start <= End;

    public static TextSpan operator +(TextSpan left, TextSpan right) => Add(left, right);

    public static TextSpan Add(TextSpan left, TextSpan right)
    {
        var start = Math.Min(left.Start, right.Start);
        var end = Math.Max(left.End, right.End);
        return new(start, end - start);
    }
}
