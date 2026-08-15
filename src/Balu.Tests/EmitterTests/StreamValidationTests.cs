using System;
using System.IO;
using Balu.Syntax;
using Balu.Text;
using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_DebugSymbols_RejectIdenticalOutputStreamsWithoutModification()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse(SourceText.From("function main() {}", "main.b")));
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var originalContent = stream.ToArray();

        var exception = Assert.Throws<ArgumentException>("symbolStream", () =>
            compilation.Emit("DebugSymbols", ReferenceProvider.References, stream, stream));

        Assert.Equal("The symbol stream must be a different instance than the output stream. (Parameter 'symbolStream')", exception.Message);
        Assert.Equal(originalContent, stream.ToArray());
    }
}
