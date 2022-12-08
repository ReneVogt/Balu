using System;
using Balu.Syntax;
using System.Collections.Generic;
using Xunit;
using System.Linq;

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
            foreach (var child in n.Children.Reverse()) stack.Push(child);
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
