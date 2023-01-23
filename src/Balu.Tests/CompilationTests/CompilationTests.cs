using System;
using Balu.Diagnostics;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.CompilationTests;

public sealed class CompilationTests
{
    [Fact]
    public void Compilation_ThrowsIfCombinedWithFailed()
    {
        var compilation = Compilation.CreateScript(null, SyntaxTree.Parse("function a() { var x = y }"));
        Assert.True(compilation.Diagnostics.HasErrors());
        Assert.Throws<ArgumentException>("previous", () => Compilation.CreateScript(compilation, SyntaxTree.Parse("function a() { var x = y }")));
    }
}