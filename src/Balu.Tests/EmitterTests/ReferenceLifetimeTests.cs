using System;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Emit;
using Balu.Syntax;
using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_ReleasesReferencesAfterSuccessfulEmit()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterReferences-");
        try
        {
            var references = CopyReferences(directory);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            using var output = new MemoryStream();

            var diagnostics = compilation.Emit("ReferenceLifetime", references, output, null);

            Assert.False(diagnostics.HasErrors());
            AssertCanOpenExclusively(references);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_ReleasesReferencesAfterPartialLoadFailure()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterReferences-");
        try
        {
            var validReference = Path.Combine(directory.FullName, Path.GetFileName(ReferenceProvider.References[0]));
            File.Copy(ReferenceProvider.References[0], validReference);
            var invalidReference = Path.Combine(directory.FullName, "invalid.dll");
            File.WriteAllText(invalidReference, "not an assembly");
            var references = new[] { validReference, invalidReference };
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            using var output = new MemoryStream();

            var diagnostics = compilation.Emit("ReferenceLifetime", references, output, null);

            Assert.Contains(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.InvalidAssemblyReference);
            AssertCanOpenExclusively(references);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_ReleasesReferencesAfterValidationFailure()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterReferences-");
        try
        {
            var reference = Path.Combine(directory.FullName, Path.GetFileName(ReferenceProvider.References[0]));
            File.Copy(ReferenceProvider.References[0], reference);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            using var output = new MemoryStream();

            var diagnostics = compilation.Emit("ReferenceLifetime", new[] { reference }, output, null);

            Assert.Contains(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.RequiredTypeNotFound);
            AssertCanOpenExclusively(new[] { reference });
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void EmitReferenceSet_DisposeIsIdempotentAndRejectsFurtherUse()
    {
        var references = new EmitReferenceSet(ReferenceProvider.References);
        references.Dispose();
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        using var output = new MemoryStream();

        references.Dispose();
        Assert.Throws<ObjectDisposedException>(() => compilation.EmitWithReferenceSet("DisposedReferences", references, output, null));
    }

    static string[] CopyReferences(DirectoryInfo directory) =>
        ReferenceProvider.References.Select(reference =>
        {
            var copy = Path.Combine(directory.FullName, Path.GetFileName(reference));
            File.Copy(reference, copy);
            return copy;
        }).ToArray();

    static void AssertCanOpenExclusively(string[] references)
    {
        foreach (var reference in references)
            using (File.Open(reference, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
    }
}
