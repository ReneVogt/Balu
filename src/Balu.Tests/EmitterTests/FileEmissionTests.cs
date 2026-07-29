using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Emit;
using Balu.Syntax;
using TestHelpers;
using Mono.Cecil;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    const string AssemblySentinel = "existing assembly";
    const string SymbolSentinel = "existing symbols";

    [Fact]
    public void Emitter_ProgramFailure_PreservesExistingFiles()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var (outputPath, symbolPath) = WriteSentinels(directory);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() { missing() }"));

            var diagnostics = compilation.Emit("ProgramFailure", ReferenceProvider.References, outputPath, symbolPath);

            Assert.True(diagnostics.HasErrors());
            AssertSentinels(outputPath, symbolPath);
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_ReferenceFailure_PreservesExistingFiles()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var (outputPath, symbolPath) = WriteSentinels(directory);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            var missingReference = Path.Combine(directory.FullName, "missing-reference.dll");

            var diagnostics = compilation.Emit("ReferenceFailure", new[] { missingReference }, outputPath, symbolPath);

            Assert.Contains(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.InvalidAssemblyReference);
            AssertSentinels(outputPath, symbolPath);
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_EmitFailure_PreservesExistingFiles()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var (outputPath, symbolPath) = WriteSentinels(directory);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            using var references = new EmitReferenceSet(ReferenceProvider.References);

            var diagnostics = compilation.EmitWithReferenceSet("EmitFailure", references, outputPath, symbolPath);

            Assert.Contains(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.SourceDocumentNameMissing);
            AssertSentinels(outputPath, symbolPath);
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_Success_ReplacesExistingFiles()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "source.b");
            File.WriteAllText(sourcePath, "function main() {}");
            var (outputPath, symbolPath) = WriteSentinels(directory);
            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));

            var diagnostics = compilation.Emit("SuccessfulEmit", ReferenceProvider.References, outputPath, symbolPath);

            Assert.False(diagnostics.HasErrors());
            Assert.NotEqual(AssemblySentinel, File.ReadAllText(outputPath));
            Assert.NotEqual(SymbolSentinel, File.ReadAllText(symbolPath));
            using var assembly = AssemblyDefinition.ReadAssembly(outputPath, new ReaderParameters { ReadSymbols = true });
            Assert.True(assembly.MainModule.HasSymbols);
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_OutputCommitFailure_RestoresExistingSymbols()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "source.b");
            File.WriteAllText(sourcePath, "function main() {}");
            var outputPath = Directory.CreateDirectory(Path.Combine(directory.FullName, "program.dll")).FullName;
            var symbolPath = Path.Combine(directory.FullName, "program.pdb");
            File.WriteAllText(symbolPath, SymbolSentinel);
            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));

            Assert.ThrowsAny<IOException>(() => compilation.Emit("CommitFailure", ReferenceProvider.References, outputPath, symbolPath));

            Assert.True(Directory.Exists(outputPath));
            Assert.Equal(SymbolSentinel, File.ReadAllText(symbolPath));
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_RejectsCanonicalOutputAndSymbolPathCollision()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var outputPath = Path.Combine(directory.FullName, "program.dll");
            File.WriteAllText(outputPath, AssemblySentinel);
            var equivalentPath = Path.Combine(directory.FullName, "subdirectory", "..", "program.dll");
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));

            var diagnostics = compilation.Emit("PathCollision", ReferenceProvider.References, outputPath, equivalentPath);

            Assert.Equal(DiagnosticId.EmitPathCollision, Assert.Single(diagnostics).Id);
            Assert.Equal(AssemblySentinel, File.ReadAllText(outputPath));
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_RejectsSourceAndOutputPathCollision()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "source.b");
            const string source = "function main() {}";
            File.WriteAllText(sourcePath, source);
            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));

            var diagnostics = compilation.Emit("PathCollision", ReferenceProvider.References, sourcePath, null);

            Assert.Equal(DiagnosticId.EmitPathCollision, Assert.Single(diagnostics).Id);
            Assert.Equal(source, File.ReadAllText(sourcePath));
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Emitter_RejectsSourceAndSymbolPathCollision()
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterFiles-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "source.b");
            const string source = "function main() {}";
            File.WriteAllText(sourcePath, source);
            var outputPath = Path.Combine(directory.FullName, "program.dll");
            File.WriteAllText(outputPath, AssemblySentinel);
            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));

            var diagnostics = compilation.Emit("PathCollision", ReferenceProvider.References, outputPath, sourcePath);

            Assert.Equal(DiagnosticId.EmitPathCollision, Assert.Single(diagnostics).Id);
            Assert.Equal(source, File.ReadAllText(sourcePath));
            Assert.Equal(AssemblySentinel, File.ReadAllText(outputPath));
            AssertNoTemporaryFiles(directory);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    static (string outputPath, string symbolPath) WriteSentinels(DirectoryInfo directory)
    {
        var outputPath = Path.Combine(directory.FullName, "program.dll");
        var symbolPath = Path.Combine(directory.FullName, "program.pdb");
        File.WriteAllText(outputPath, AssemblySentinel);
        File.WriteAllText(symbolPath, SymbolSentinel);
        return (outputPath, symbolPath);
    }

    static void AssertSentinels(string outputPath, string symbolPath)
    {
        Assert.Equal(AssemblySentinel, File.ReadAllText(outputPath));
        Assert.Equal(SymbolSentinel, File.ReadAllText(symbolPath));
    }

    static void AssertNoTemporaryFiles(DirectoryInfo directory) =>
        Assert.Empty(directory.EnumerateFiles().Where(file => file.Extension is ".tmp" or ".bak"));
}
