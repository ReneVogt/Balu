using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax;

/// <summary>
/// Represents a list of syntax nodes separated by tokens.
/// For example: the parameter list of a function call.
/// </summary>
/// <typeparam name="T">The type of <see cref="SyntaxNode"/>.</typeparam>
public sealed class SeparatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
{
    /// <summary>
    /// The number of separated <see cref="SyntaxNode"/>s (without the separating nodes).
    /// </summary>
    public int Count { get; }
    /// <summary>
    /// The <see cref="SyntaxNode"/> at a given position (ignoring the separators).
    /// </summary>
    /// <param name="index">The position in the separated list.</param>
    /// <returns>The <typeparamref name="T"/> at the given <paramref name="index"/>.</returns>
    public T this[int index] => (T)ElementsWithSeparators[index * 2];

    public ImmutableArray<SyntaxNode> ElementsWithSeparators { get; }

    /// <summary>
    /// Creates a new <see cref="SeparatedSyntaxList{T}"/> from the given <see cref="SyntaxNode"/> sequence.
    /// </summary>
    /// <param name="separatorsAndNodes">The sequence of <see cref="SyntaxNode"/> and their separating <see cref="SyntaxToken"/>.</param>
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
