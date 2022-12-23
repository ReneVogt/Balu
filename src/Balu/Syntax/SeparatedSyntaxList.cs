using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

public sealed class SeparatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
{
    public int Count { get; }
    public T this[int index] => (T)ElementsWithSeparators[index * 2];

    public ImmutableArray<SyntaxNode> ElementsWithSeparators { get; }

    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> separatorsAndNodes)
    {
        ElementsWithSeparators = separatorsAndNodes;
        Count = (ElementsWithSeparators.Length + 1) / 2;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++) yield return this[i];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
