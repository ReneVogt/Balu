using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Balu.Diagnostics;
using Balu.Interpretation;
using Balu.Tests.TestHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Balu.Tests.InterpreterTests;

public sealed class InterpreterTests
{
    [Fact]
    public void Interpreter_WriteSyntax_WritesToOut()
    {
        using var output = new StringWriter();
        using var interpreter = new Interpreter(ReferenceProvider.References)
        {
            Out = output,
            WriteSyntax = true
        };

        var diagnostics = interpreter.Execute("1");

        Assert.False(diagnostics.HasErrors());
        Assert.Contains("Syntax:", output.ToString());
        Assert.Contains("CompilationUnit", output.ToString());
    }

    [Fact]
    public void Interpreter_WriteProgram_WritesToOut()
    {
        using var output = new StringWriter();
        using var interpreter = new Interpreter(ReferenceProvider.References)
        {
            Out = output,
            WriteProgram = true
        };

        var diagnostics = interpreter.Execute("1");

        Assert.False(diagnostics.HasErrors());
        Assert.Contains("Program:", output.ToString());
        Assert.Contains("function <eval>()", output.ToString());
    }

    [Fact]
    public void Interpreter_CopiesReferences()
    {
        var references = ReferenceProvider.References.ToArray();
        using var interpreter = new Interpreter(references);
        references[0] = null!;

        var diagnostics = interpreter.Execute("1");

        Assert.False(diagnostics.HasErrors());
    }

    [Fact]
    public void Interpreter_SubmissionNames_AdvanceOnlyForAcceptedCompilationsAndReset()
    {
        using var interpreter = new Interpreter(ReferenceProvider.References);

        var invalidDiagnostics = interpreter.Execute("function invalid() { missing() }");
        var validDiagnostics = interpreter.Execute("function valid() {}");

        Assert.True(invalidDiagnostics.HasErrors());
        Assert.False(validDiagnostics.HasErrors());
        Assert.Equal("BaluInterpreter/submission-0001.b", Assert.Single(interpreter.Compilation.SyntaxTrees).Text.FileName);

        interpreter.Reset();
        var resetDiagnostics = interpreter.Execute("function afterReset() {}");

        Assert.False(resetDiagnostics.HasErrors());
        Assert.Equal("BaluInterpreter/submission-0001.b", Assert.Single(interpreter.Compilation.SyntaxTrees).Text.FileName);
        var directory = Directory.CreateTempSubdirectory("BaluInterpreterReset-");
        try
        {
            var outputPath = Path.Combine(directory.FullName, "program.dll");
            var emitDiagnostics = interpreter.Emit(outputPath, Path.ChangeExtension(outputPath, ".pdb"));

            Assert.False(emitDiagnostics.HasErrors());
            using var assembly = AssemblyDefinition.ReadAssembly(outputPath, new ReaderParameters { ReadSymbols = true });
            var documentNames = assembly.MainModule.Types
                                        .SelectMany(type => type.Methods)
                                        .SelectMany(method => method.DebugInformation.SequencePoints)
                                        .Select(sequencePoint => sequencePoint.Document.Url)
                                        .Distinct()
                                        .ToArray();
            Assert.Equal(new[] { "BaluInterpreter/submission-0001.b" }, documentNames);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Interpreter_EmitDebug_DescribesAllSubmissions()
    {
        const string firstSource = "function first() { println(\"first\") }";
        const string secondSource = "function second() { first() }";
        using var interpreter = new Interpreter(ReferenceProvider.References);
        Assert.False(interpreter.Execute(firstSource).HasErrors());
        Assert.False(interpreter.Execute(secondSource).HasErrors());
        var directory = Directory.CreateTempSubdirectory("BaluInterpreter-");
        try
        {
            var outputPath = Path.Combine(directory.FullName, "program.dll");
            var symbolPath = Path.ChangeExtension(outputPath, ".pdb");

            var diagnostics = interpreter.Emit(outputPath, symbolPath);

            Assert.Empty(diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
            using var assembly = AssemblyDefinition.ReadAssembly(outputPath, new ReaderParameters { ReadSymbols = true });
            var documents = assembly.MainModule.Types
                                    .SelectMany(type => type.Methods)
                                    .SelectMany(method => method.DebugInformation.SequencePoints)
                                    .Select(sequencePoint => sequencePoint.Document)
                                    .Distinct()
                                    .OrderBy(document => document.Url)
                                    .ToArray();
            Assert.Equal(
                new[] { "BaluInterpreter/submission-0001.b", "BaluInterpreter/submission-0002.b" },
                documents.Select(document => document.Url));
            Assert.All(documents, document => Assert.Equal(DocumentHashAlgorithm.SHA256, document.HashAlgorithm));
            using var algorithm = SHA256.Create();
            Assert.Equal(algorithm.ComputeHash(Encoding.UTF8.GetBytes(firstSource)), documents[0].Hash);
            Assert.Equal(algorithm.ComputeHash(Encoding.UTF8.GetBytes(secondSource)), documents[1].Hash);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Interpreter_UsesOnlyErrorFreeCompilations()
    {
        using var asserter = new CompilationAsserter();
        asserter.AssertScriptEvaluation("function a() { var x = [y] }", expectedDiagnostics: "Undefined name 'y'.");
        asserter.AssertScriptEvaluation("function c() : int { return 42 }");
        asserter.AssertScriptEvaluation("c()", value: 42);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("true", true)]
    [InlineData("\"text\"", "text")]
    public void Interpreter_PersistsAnyGlobalsAcrossSubmissions(string literal, object expected)
    {
        using var asserter = new CompilationAsserter();

        asserter.AssertScriptEvaluation($"var value: any = {literal}");
        asserter.AssertScriptEvaluation("value", value: expected);
    }

    [Fact]
    public void Interpreter_RuntimeFailure_RollsBackSubmission()
    {
        using var interpreter = new Interpreter(ReferenceProvider.References);
        Assert.False(interpreter.Execute("var value = 42 value").HasErrors());
        var compilation = interpreter.Compilation;
        var globalVariables = interpreter.GlobalVariables;

        var exception = Assert.Throws<TargetInvocationException>(
            () => interpreter.Execute("value = 0 var divisor = 0 value / divisor"));

        Assert.IsType<System.DivideByZeroException>(exception.InnerException);
        Assert.Same(compilation, interpreter.Compilation);
        Assert.Same(globalVariables, interpreter.GlobalVariables);
        Assert.Equal(42, interpreter.Result);
        var globalVariable = Assert.Single(interpreter.GlobalVariables);
        Assert.Equal("value", globalVariable.Key.Name);
        Assert.Equal(42, globalVariable.Value);
        Assert.DoesNotContain(interpreter.AllSymbols, symbol => symbol.Name == "divisor");

        Assert.False(interpreter.Execute("value").HasErrors());
        Assert.Equal("BaluInterpreter/submission-0002.b", Assert.Single(interpreter.Compilation.SyntaxTrees).Text.FileName);
        Assert.Equal(42, interpreter.Result);
    }

    [Fact]
    public void Interpreter_InputAtEndOfStream_ReturnsEmptyString()
    {
        var originalIn = System.Console.In;
        try
        {
            System.Console.SetIn(new StringReader(string.Empty));
            using var interpreter = new Interpreter(ReferenceProvider.References);

            Assert.False(interpreter.Execute("var value = input()").HasErrors());
            Assert.Equal(string.Empty, Assert.Single(interpreter.GlobalVariables).Value);
            Assert.False(interpreter.Execute("value").HasErrors());
            Assert.Equal(string.Empty, interpreter.Result);
        }
        finally
        {
            System.Console.SetIn(originalIn);
        }
    }

    [Fact]
    public void Interpreter_Reset_ClearsResultAndGlobalVariables()
    {
        using var interpreter = new Interpreter(ReferenceProvider.References);
        Assert.False(interpreter.Execute("var value = 42 value").HasErrors());

        interpreter.Reset();

        Assert.Null(interpreter.Result);
        Assert.Empty(interpreter.GlobalVariables);
        Assert.DoesNotContain(interpreter.AllSymbols, symbol => symbol.Name == "value");
        var diagnostics = interpreter.Execute("value");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == DiagnosticId.UndefinedName);
        Assert.Null(interpreter.Result);
        Assert.Empty(interpreter.GlobalVariables);
    }

    [Fact]
    public void Interpreter_ReusesReferenceSnapshot()
    {
        var directory = Directory.CreateTempSubdirectory("BaluInterpreterReferences-");
        try
        {
            var references = ReferenceProvider.References.Select(reference =>
            {
                var copy = Path.Combine(directory.FullName, Path.GetFileName(reference));
                File.Copy(reference, copy);
                return copy;
            }).ToArray();
            using var interpreter = new Interpreter(references);

            Assert.False(interpreter.Execute("1").HasErrors());
            foreach (var reference in references)
                File.Delete(reference);

            Assert.False(interpreter.Execute("2").HasErrors());
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

}
