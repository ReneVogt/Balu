using System;
using Balu.Syntax;
using System.Collections.Generic;
using Xunit;

namespace Balu.Tests.TestHelper;

sealed class SyntaxTreeAsserter : IDisposable
{
    readonly IEnumerator<SyntaxNode> enumerator;
    bool hasErrors;

    public SyntaxTreeAsserter(SyntaxNode node) => enumerator = Flatten(node).GetEnumerator();
    public void Dispose()
    {
        if (!hasErrors)
            Assert.False(enumerator.MoveNext());
        enumerator.Dispose();
    }

    static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
    {
        Stack<SyntaxNode> stack = new();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;
            for(int i = n.ChildrenCount-1; i>= 0; i--) stack.Push(n.GetChild(i));
        }
    }

    bool Markfailed() => !(hasErrors = true);

    public void AssertToken(SyntaxKind kind)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(kind, enumerator.Current.Kind);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertToken(SyntaxKind kind, string text)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(kind, enumerator.Current.Kind);
            var token = Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(text, token.Text);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertToken(SyntaxKind kind, string text, object? value)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(kind, enumerator.Current.Kind);
            var token = Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertNode(SyntaxKind kind)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(kind, enumerator.Current.Kind);
            Assert.IsNotType<SyntaxToken>(enumerator.Current);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
}
