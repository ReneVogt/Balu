using System;
using System.IO;
using System.Threading;
using Balu.Syntax;
using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests;

public sealed class CancellationTests
{
    [Fact]
    public void SyntaxTree_Parse_ThrowsIfCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() => SyntaxTree.Parse("1", cancellation.Token));
    }

    [Fact]
    public void Compilation_Create_ThrowsIfCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() => Compilation.Create(cancellation.Token, SyntaxTree.Parse("function main() {}")));
    }

    [Fact]
    public void Compilation_Emit_ThrowsIfCancellationRequested()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        using var output = new MemoryStream();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => compilation.Emit("Canceled", ReferenceProvider.References, output, null, cancellation.Token));
    }
}
