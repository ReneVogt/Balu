using System;
using Balu.Syntax;
using System.Collections.Generic;
using System.Linq;
using Balu.Text;
using Xunit;

namespace Balu.Tests.TestHelper;

sealed class SyntaxTreeAsserter : IDisposable
{
    readonly IEnumerator<object> enumerator;
    readonly bool includeTrivia;
    bool hasErrors;

    public SyntaxTreeAsserter(SyntaxNode node, bool includeTrivia = false)
    {
        this.includeTrivia = includeTrivia;
        enumerator = Flatten(node).GetEnumerator();
    }
    public void Dispose()
    {
        if (!hasErrors)
            Assert.False(enumerator.MoveNext());
        enumerator.Dispose();
    }

    IEnumerable<object> Flatten(SyntaxNode node)
    {
        if (node.SyntaxTree.Diagnostics.Any())
        {
            foreach (var diagnostic in node.SyntaxTree.Diagnostics)
                yield return diagnostic;
            yield break;
        }

        Stack<SyntaxNode> stack = new();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (includeTrivia && n is SyntaxToken t1)
                foreach (var trivia in t1.LeadingTrivia)
                    yield return trivia;
            yield return n;
            if (includeTrivia && n is SyntaxToken t2)
                foreach (var trivia in t2.TrailingTrivia)
                    yield return trivia;
            for (int i = n.ChildrenCount-1; i>= 0; i--) stack.Push(n.GetChild(i));
        }
    }

    bool Markfailed() => !(hasErrors = true);

    public void AsssertDiagnostic(string message)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            var diagnostic = Assert.IsType<Diagnostic>(enumerator.Current);
            Assert.Equal(message, diagnostic.Message);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AsssertDiagnostic(string message, TextLocation location)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            var diagnostic = Assert.IsType<Diagnostic>(enumerator.Current);
            Assert.Equal(message, diagnostic.Message);
            Assert.Equal(location.Span, diagnostic.Location.Span);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertTrivia(SyntaxKind kind)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            var trivia = Assert.IsType<SyntaxTrivia>(enumerator.Current);
            Assert.Equal(kind, trivia.Kind);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertTrivia(SyntaxKind kind, string text)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            var trivia = Assert.IsType<SyntaxTrivia>(enumerator.Current);
            Assert.Equal(kind, trivia.Kind);
            Assert.Equal(text, trivia.Text);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
    public void AssertToken(SyntaxKind kind)
    {
        try
        {
            Assert.True(enumerator.MoveNext());
            var token = Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(kind, token.Kind);
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
            var token = Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(kind, token.Kind);
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
            var token = Assert.IsType<SyntaxToken>(enumerator.Current);
            Assert.Equal(kind, token.Kind);
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
            var node = Assert.IsAssignableFrom<SyntaxNode>(enumerator.Current);
            Assert.Equal(kind, node.Kind);
            Assert.IsNotType<SyntaxToken>(node);
        }
        catch when (Markfailed())
        {
            throw;
        }
    }
}
