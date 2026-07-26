using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Tests.TestHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
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
            File.WriteAllBytes(sourcePath, encoding.GetPreamble().Concat(encoding.GetBytes(source)).ToArray());

            var compilation = Compilation.Create(SyntaxTree.Load(sourcePath));
            var diagnostics = compilation.Emit("DebugSymbols", ReferenceProvider.References, outputPath, symbolPath);

            Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
            Assert.True(File.Exists(outputPath));
            Assert.True(File.Exists(symbolPath));

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
