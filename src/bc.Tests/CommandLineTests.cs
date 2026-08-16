using System;
using System.IO;
using System.Text;
using Xunit;

namespace Balu.Compiler.Tests;

public sealed class CommandLineTests
{
    [Fact]
    public void Compiler_MissingOptionValue_ReturnsInvocationError()
    {
        var (ExitCode, _, Error)= Run(["-o"]);

        Assert.Equal(2, ExitCode);
        Assert.StartsWith("bc: error: ", Error);
        Assert.DoesNotContain("Exception", Error);
    }

    [Fact]
    public void Compiler_UnknownOption_ReturnsInvocationError()
    {
        var (ExitCode, Output, Error)= Run(["--bogus"]);

        Assert.Equal(2, ExitCode);
        Assert.StartsWith("bc: error: ", Error);
        Assert.DoesNotContain("Compiling", Output);
    }

    [Fact]
    public void Compiler_UnknownSlashOption_IsPlatformAppropriate()
    {
        var (ExitCode, Output, Error)= Run(["/bogus"]);

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(2, ExitCode);
            Assert.StartsWith("bc: error: Unknown option '/bogus'.", Error);
            Assert.DoesNotContain("Compiling", Output);
        }
        else
        {
            Assert.Equal(3, ExitCode);
            Assert.Contains("Compiling '/bogus'", Output);
            Assert.DoesNotContain("Unknown option", Error);
        }
    }

    [Fact]
    public void Compiler_OptionTerminator_AllowsDashPrefixedSourcePath()
    {
        var (ExitCode, Output, Error)= Run(["--", "-program.b"]);

        Assert.Equal(3, ExitCode);
        Assert.Contains("Compiling '-program.b'", Output);
        Assert.DoesNotContain("Unknown option", Error);
    }

    [Fact]
    public void Compiler_ResponseFile_ReadsQuotedUtf8SourcePath()
    {
        var directory = Directory.CreateTempSubdirectory("BaluCompilerTests-");
        try
        {
            var sourceDirectory = Directory.CreateDirectory(Path.Combine(directory.FullName, "source files-\u00E4"));
            var sourcePath = Path.Combine(sourceDirectory.FullName, "program.b");
            var responsePath = Path.Combine(directory.FullName, "arguments.rsp");
            File.WriteAllText(sourcePath, "missing()");
            File.WriteAllText(responsePath, $"\"{sourcePath}\"", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            var (ExitCode, Output, Error)= Run([$"@{responsePath}"]);

            Assert.Equal(1, ExitCode);
            Assert.Contains($"Compiling '{sourcePath}'", Output);
            Assert.DoesNotContain("bc: error:", Error);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Compiler_MissingResponseFile_ReturnsInvocationError()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.rsp");

        var (ExitCode, Output, Error)= Run([$"@{missingPath}"]);

        Assert.Equal(2, ExitCode);
        Assert.Empty(Output);
        Assert.StartsWith("bc: error: ", Error);
        Assert.Contains(missingPath, Error);
    }

    [Fact]
    public void Compiler_NoSourceFiles_ReturnsInvocationError()
    {
        var (ExitCode, _, Error)= Run([]);

        Assert.Equal(2, ExitCode);
        Assert.Contains("bc: error: need at least one source file.", Error);
    }

    [Fact]
    public void Compiler_QuietInvocationError_SuppressesOutput()
    {
        var (ExitCode, Output, Error)= Run(["-o", "-q"]);

        Assert.Equal(2, ExitCode);
        Assert.Empty(Output);
        Assert.Empty(Error);
    }

    [Fact]
    public void Compiler_MissingSourceFile_ReturnsToolError()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.b");

        var (ExitCode, Output, Error)= Run([missingPath, "-q"]);

        Assert.Equal(3, ExitCode);
        Assert.Empty(Output);
        Assert.Empty(Error);
    }

    [Fact]
    public void Compiler_MultipleMissingSourceFiles_ReportsEachError()
    {
        var firstMissingPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.b");
        var secondMissingPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.b");

        var (ExitCode, _, Error)= Run([firstMissingPath, secondMissingPath]);

        Assert.Equal(3, ExitCode);
        Assert.Contains(firstMissingPath, Error);
        Assert.Contains(secondMissingPath, Error);
        Assert.DoesNotContain("One or more errors occurred", Error);
    }

    [Fact]
    public void Compiler_SourceDiagnostic_ReturnsCompilationError()
    {
        var directory = Directory.CreateTempSubdirectory("BaluCompilerTests-");
        try
        {
            var sourcePath = Path.Combine(directory.FullName, "program.b");
            File.WriteAllText(sourcePath, "function main() { missing() }");

            var (ExitCode, Output, Error)= Run([sourcePath, "-q"]);

            Assert.Equal(1, ExitCode);
            Assert.Empty(Output);
            Assert.Empty(Error);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Compiler_Help_ReturnsSuccess()
    {
        var (ExitCode, Output, Error)= Run(["--help"]);

        Assert.Equal(0, ExitCode);
        Assert.Contains("Usage: bc <source-paths> [options]", Output);
        Assert.Empty(Error);
    }

    static (int ExitCode, string Output, string Error) Run(string[] arguments)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var program = new global::Program(output, error);

        var exitCode = program.Run(arguments);

        return (exitCode, output.ToString(), error.ToString());
    }
}
