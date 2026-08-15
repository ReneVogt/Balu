using System.IO;
using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.UnitTests.Diagnostics;

public class DiagnosticTests
{
    [Fact]
    public void Diagnostic_ToString_FormatsNoEntryPointWithoutLocation()
    {
        var diagnostic = Assert.Single(Compilation.Create().Diagnostics);

        Assert.Equal(DiagnosticId.NoEntryPointDefined, diagnostic.Id);
        Assert.False(diagnostic.HasLocation);
        Assert.Equal($"[{diagnostic.IdString}] {diagnostic.Message}", diagnostic.ToString());
    }

    [Fact]
    public void Diagnostic_ToString_FormatsInvalidReferenceWithoutLocation()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        using var output = new MemoryStream();

        var diagnostic = Assert.Single(compilation.Emit("InvalidReference", [string.Empty], output, null));

        Assert.Equal(DiagnosticId.InvalidAssemblyReference, diagnostic.Id);
        Assert.False(diagnostic.HasLocation);
        Assert.Equal($"[{diagnostic.IdString}] {diagnostic.Message}", diagnostic.ToString());
    }

    [Fact]
    public void Diagnostic_ToString_PreservesLocatedFormat()
    {
        var source = SourceText.From("\"", "test.b");
        SyntaxTree.ParseTokens(source, out var diagnostics);
        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal(DiagnosticId.UnterminatedString, diagnostic.Id);
        Assert.True(diagnostic.HasLocation);
        Assert.Equal($"test.b(1,1): [{diagnostic.IdString}] {diagnostic.Message}", diagnostic.ToString());
    }
}
