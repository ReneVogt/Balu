using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Balu.Diagnostics;
using Balu.Syntax;
using TestHelpers;
using Balu.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_DebugSymbols_RequireDocumentName()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        using var outputStream = new MemoryStream();
        using var symbolStream = new MemoryStream();

        var diagnostics = compilation.Emit("DebugSymbols", ReferenceProvider.References, outputStream, symbolStream);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.SourceDocumentNameMissing, diagnostic.Id);
        Assert.Equal("Cannot emit debug symbols for a source document without a name.", diagnostic.Message);
        Assert.Equal(0, outputStream.Length);
        Assert.Equal(0, symbolStream.Length);
    }

    [Fact]
    public void Emitter_DebugSymbols_RejectDocumentNameCollision()
    {
        var helper = SyntaxTree.Parse(SourceText.From("function helper() {}", "shared.b"));
        var main = SyntaxTree.Parse(SourceText.From("function main() { helper() }", "shared.b"));
        var compilation = Compilation.Create(helper, main);
        using var outputStream = new MemoryStream();
        using var symbolStream = new MemoryStream();

        var diagnostics = compilation.Emit("DebugSymbols", ReferenceProvider.References, outputStream, symbolStream);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.SourceDocumentNameCollision, diagnostic.Id);
        Assert.Equal("Cannot emit debug symbols because document name 'shared.b' identifies different source texts.", diagnostic.Message);
        Assert.Equal(0, outputStream.Length);
        Assert.Equal(0, symbolStream.Length);
    }

    [Fact]
    public void Emitter_DebugSymbols_DescribeMultipleInMemoryDocuments()
    {
        const string helpersSource = "function first() {} function second() {}";
        const string mainSource = "function main() { first() second() }";
        var helpers = SyntaxTree.Parse(SourceText.From(helpersSource, "helpers.b"));
        var main = SyntaxTree.Parse(SourceText.From(mainSource, "main.b"));
        var compilation = Compilation.Create(helpers, main);
        using var outputStream = new MemoryStream();
        using var symbolStream = new MemoryStream();

        var diagnostics = compilation.Emit("DebugSymbols", ReferenceProvider.References, outputStream, symbolStream);

        Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        outputStream.Position = 0;
        symbolStream.Position = 0;
        using var assembly = AssemblyDefinition.ReadAssembly(
            outputStream,
            new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new PortablePdbReaderProvider(), SymbolStream = symbolStream });
        var documents = assembly.MainModule.Types
                                .SelectMany(type => type.Methods)
                                .SelectMany(method => method.DebugInformation.SequencePoints)
                                .Select(sequencePoint => sequencePoint.Document)
                                .Distinct()
                                .OrderBy(document => document.Url)
                                .ToArray();
        Assert.Equal(new[] { "helpers.b", "main.b" }, documents.Select(document => document.Url));
        using var algorithm = SHA256.Create();
        Assert.Equal(algorithm.ComputeHash(Encoding.UTF8.GetBytes(helpersSource)), documents[0].Hash);
        Assert.Equal(algorithm.ComputeHash(Encoding.UTF8.GetBytes(mainSource)), documents[1].Hash);
    }

    [Fact]
    public void Emitter_DebugSymbols_RecognizeLoneCarriageReturnAsLineEnding()
    {
        const string source = "function main() {\r    println(\"a\")\r}";
        var compilation = Compilation.Create(SyntaxTree.Parse(SourceText.From(source, "lone-cr.b")));
        using var outputStream = new MemoryStream();
        using var symbolStream = new MemoryStream();

        var diagnostics = compilation.Emit("LoneCrDebugSymbols", ReferenceProvider.References, outputStream, symbolStream);

        Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        outputStream.Position = 0;
        symbolStream.Position = 0;
        using var assembly = AssemblyDefinition.ReadAssembly(
            outputStream,
            new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new PortablePdbReaderProvider(), SymbolStream = symbolStream });
        var sequencePoints = assembly.MainModule.Types
                                     .SelectMany(type => type.Methods)
                                     .SelectMany(method => method.DebugInformation.SequencePoints)
                                     .ToArray();

        Assert.Contains(sequencePoints, sequencePoint => sequencePoint.StartLine == 2);
        Assert.All(sequencePoints, sequencePoint => Assert.InRange(sequencePoint.StartLine, 1, 3));
    }

    [Fact]
    public void Emitter_WithoutDebugSymbols_AllowsMissingDocumentName()
    {
        var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
        using var outputStream = new MemoryStream();

        var diagnostics = compilation.Emit("NoDebugSymbols", ReferenceProvider.References, outputStream, null);

        Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.True(outputStream.Length > 0);
    }

    [Fact]
    public void Emitter_DebugSymbols_AreDiscoverableAndDescribeSourceFile()
    {
        var directory = Directory.CreateTempSubdirectory("Balu-Ren\u00E9-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "source.b");
            var outputPath = Path.Combine(directory.FullName, "program.dll");
            var symbolPath = Path.ChangeExtension(outputPath, ".pdb");
            const string source = "println(\"Ren\u00E9\")";
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            File.WriteAllBytes(sourcePath, [.. encoding.GetPreamble(), .. encoding.GetBytes(source)]);

            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));
            var diagnostics = compilation.Emit("DebugSymbols", ReferenceProvider.References, outputPath, symbolPath);

            Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
            Assert.True(File.Exists(outputPath));
            Assert.True(File.Exists(symbolPath));

            using var assemblyWithoutSymbols = AssemblyDefinition.ReadAssembly(outputPath);
            var codeView = Assert.Single(assemblyWithoutSymbols.MainModule.GetDebugHeader().Entries.Where(entry => entry.Directory.Type == ImageDebugType.CodeView));
            Assert.Equal(symbolPath, Encoding.UTF8.GetString(codeView.Data, 24, codeView.Data.Length - 25));

            using var assembly = AssemblyDefinition.ReadAssembly(outputPath, new ReaderParameters { ReadSymbols = true });
            Assert.True(assembly.MainModule.HasSymbols);
            var sequencePoints = assembly.MainModule.Types
                                         .SelectMany(type => type.Methods)
                                         .SelectMany(method => method.DebugInformation.SequencePoints)
                                         .ToArray();
            Assert.NotEmpty(sequencePoints);
            Assert.All(sequencePoints, sequencePoint => Assert.Equal(sourcePath, sequencePoint.Document.Url));

            var document = sequencePoints[0].Document;
            Assert.Equal(DocumentHashAlgorithm.SHA256, document.HashAlgorithm);
            using var algorithm = SHA256.Create();
            Assert.Equal(algorithm.ComputeHash(File.ReadAllBytes(sourcePath)), document.Hash);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
