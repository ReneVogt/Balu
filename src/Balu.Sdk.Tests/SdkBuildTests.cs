using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Balu.Sdk.Tests;

public sealed class SdkBuildTests
{
    static readonly string SdkPackageId = GetAssemblyMetadata("BaluSdkPackageId");
    static readonly string SdkPackageVersion = GetAssemblyMetadata("BaluSdkPackageVersion");

    [Fact]
    public void Sdk_DefaultConfigurations_HonorSymbolAndOptimizeProperties()
    {
        using var project = new TestProject();

        project.Build("Debug");

        Assert.True(File.Exists(project.IntermediateAssembly("Debug")));
        Assert.True(File.Exists(project.IntermediatePdb("Debug")), project.DescribePdbs());
        Assert.True(File.Exists(project.OutputPdb("Debug")));
        AssertDebuggerFriendly(project.IntermediateAssembly("Debug"), expected: true, project.DescribePdbs());

        project.Build("Release");

        Assert.True(File.Exists(project.IntermediateAssembly("Release")));
        Assert.True(File.Exists(project.IntermediatePdb("Release")), project.DescribePdbs());
        Assert.True(File.Exists(project.OutputPdb("Release")));
        AssertDebuggerFriendly(project.IntermediateAssembly("Release"), expected: false);
    }

    [Theory]
    [InlineData("<DebugType>None</DebugType>")]
    [InlineData("<DebugSymbols>false</DebugSymbols><DebugType></DebugType>")]
    public void Sdk_DisabledSymbols_DoNotEmitPdb(string properties)
    {
        using var project = new TestProject(properties);

        project.Build("Debug");

        Assert.True(File.Exists(project.IntermediateAssembly("Debug")));
        Assert.False(File.Exists(project.IntermediatePdb("Debug")));
        Assert.False(File.Exists(project.OutputPdb("Debug")));
        AssertDebuggerFriendly(project.IntermediateAssembly("Debug"), expected: false, project.DescribePdbs());
    }

    [Fact]
    public void Sdk_CustomConfiguration_HonorsStandardProperties()
    {
        using var project = new TestProject("<DebugSymbols>true</DebugSymbols><DebugType>portable</DebugType><Optimize>false</Optimize>");

        project.Build("Staging");

        Assert.True(File.Exists(project.IntermediatePdb("Staging")), project.DescribePdbs());
        Assert.True(File.Exists(project.OutputPdb("Staging")));
        AssertDebuggerFriendly(project.IntermediateAssembly("Staging"), expected: true);
    }

    [Fact]
    public void Sdk_PropertyChanges_InvalidateCompilerOutputs()
    {
        using var project = new TestProject();
        project.Build("Debug");
        var assemblyPath = project.IntermediateAssembly("Debug");
        var initialWrite = File.GetLastWriteTimeUtc(assemblyPath);

        Thread.Sleep(1100);
        project.SetProperties("<DebugType>None</DebugType>");
        project.Build("Debug", noRestore: true);

        Assert.True(File.GetLastWriteTimeUtc(assemblyPath) > initialWrite);
        Assert.False(File.Exists(project.IntermediatePdb("Debug")));
        Assert.False(File.Exists(project.OutputPdb("Debug")));

        project.SetProperties("<DebugType>portable</DebugType><Optimize>true</Optimize>");
        project.Build("Debug", noRestore: true);

        Assert.True(File.Exists(project.IntermediatePdb("Debug")));
        AssertDebuggerFriendly(assemblyPath, expected: false);
    }

    [Fact]
    public void Sdk_EmbeddedSymbols_ReportUnsupportedConfiguration()
    {
        using var project = new TestProject("<DebugType>embedded</DebugType>");

        var result = project.TryBuild("Debug");

        Assert.NotEqual(0, result.exitCode);
        Assert.Contains("Balu does not support embedded PDBs", result.output);
    }

    [Fact]
    public void Sdk_DisablingSymbols_RemovesCustomPdbOutputs()
    {
        const string customSymbolPath = "obj/custom-symbols.pdb";
        using var project = new TestProject($"<BaluSymbolPath>{customSymbolPath}</BaluSymbolPath>");
        project.Build("Debug");
        var intermediatePdb = project.PathFromProject(customSymbolPath);
        var outputPdb = project.PathFromProject("bin/Debug/net10.0/custom-symbols.pdb");
        Assert.True(File.Exists(intermediatePdb));
        Assert.True(File.Exists(outputPdb));

        project.SetProperties("<DebugType>None</DebugType>");
        project.Build("Debug", noRestore: true);

        Assert.False(File.Exists(intermediatePdb));
        Assert.False(File.Exists(outputPdb));
    }

    [Fact]
    public void Sdk_FailedRecompilation_PreservesLastSuccessfulOutputs()
    {
        using var project = new TestProject();
        project.Build("Debug");
        var assemblyPath = project.IntermediateAssembly("Debug");
        var pdbPath = project.IntermediatePdb("Debug");
        var assemblyWrite = File.GetLastWriteTimeUtc(assemblyPath);
        var pdbWrite = File.GetLastWriteTimeUtc(pdbPath);

        project.SetSource("missing()");
        var result = project.TryBuild("Debug", noRestore: true);

        Assert.NotEqual(0, result.exitCode);
        Assert.True(File.Exists(assemblyPath));
        Assert.True(File.Exists(pdbPath));
        Assert.Equal(assemblyWrite, File.GetLastWriteTimeUtc(assemblyPath));
        Assert.Equal(pdbWrite, File.GetLastWriteTimeUtc(pdbPath));
    }

    [Fact]
    public void Sdk_MissingPdb_RecompilesAndCleanRemovesOutputs()
    {
        using var project = new TestProject();
        project.Build("Debug");
        var assemblyPath = project.IntermediateAssembly("Debug");
        var intermediatePdb = project.IntermediatePdb("Debug");
        var outputPdb = project.OutputPdb("Debug");
        var initialWrite = File.GetLastWriteTimeUtc(assemblyPath);

        File.Delete(intermediatePdb);
        File.Delete(outputPdb);
        Thread.Sleep(1100);
        project.Build("Debug", noRestore: true);

        Assert.True(File.Exists(intermediatePdb), project.DescribePdbs());
        Assert.True(File.Exists(outputPdb));
        var regeneratedWrite = File.GetLastWriteTimeUtc(assemblyPath);
        Assert.True(regeneratedWrite > initialWrite);

        Thread.Sleep(1100);
        project.Build("Debug", noRestore: true);
        Assert.Equal(regeneratedWrite, File.GetLastWriteTimeUtc(assemblyPath));

        project.Clean("Debug");
        Assert.False(File.Exists(assemblyPath));
        Assert.False(File.Exists(intermediatePdb));
        Assert.False(File.Exists(outputPdb));
    }

    [Fact]
    public void Sdk_DefaultItems_IgnoreIntermediateSources()
    {
        using var project = new TestProject();
        project.SetSource("obj/generated.b", "missing()");

        project.Build("Debug");
    }

    [Fact]
    public void Sdk_DefaultItems_HonorExplicitRemoves()
    {
        using var project = new TestProject(items: "<ItemGroup><BaluFiles Remove=\"excluded.b\" /></ItemGroup>");
        project.SetSource("excluded.b", "missing()");

        project.Build("Debug");
    }

    [Fact]
    public void Sdk_DeletingSourceFile_InvalidatesCompilerOutput()
    {
        using var project = new TestProject();
        project.SetSource("removed.b", "function removed(): int { return 42 }");
        project.Build("Debug");
        var assemblyPath = project.IntermediateAssembly("Debug");
        AssertProgramMethod(assemblyPath, "removed", expected: true);

        Thread.Sleep(1100);
        project.DeleteSource("removed.b");
        project.Build("Debug", noRestore: true);

        AssertProgramMethod(assemblyPath, "removed", expected: false);
    }

    static void AssertDebuggerFriendly(string assemblyPath, bool expected, string? details = null)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        var hasDebuggableAttribute = assembly.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == "System.Diagnostics.DebuggableAttribute");
        var hasNop = assembly.EntryPoint.Body.Instructions.Any(instruction => instruction.OpCode == OpCodes.Nop);
        Assert.True(hasDebuggableAttribute == expected, details);
        Assert.True(hasNop == expected, details);
    }

    static void AssertProgramMethod(string assemblyPath, string methodName, bool expected)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        var program = assembly.MainModule.Types.Single(type => type.Name == "Program");
        Assert.True(program.Methods.Any(method => method.Name == methodName) == expected);
    }

    sealed class TestProject : IDisposable
    {
        const string ProjectName = "TestProject";
        readonly string directory;
        readonly string packageCache;
        string lastBuildOutput = string.Empty;

        public TestProject(string properties = "", string items = "")
        {
            directory = Directory.CreateTempSubdirectory("BaluSdkTests-").FullName;
            packageCache = Path.Combine(directory, "packages-cache");
            var packageSource = Path.Combine(AppContext.BaseDirectory, "packages");
            File.WriteAllText(
                Path.Combine(directory, "NuGet.config"),
                $"<configuration><packageSources><clear/><add key=\"BaluSdk\" value=\"{SecurityElement.Escape(packageSource)}\"/></packageSources></configuration>");
            SetProperties(properties, items);
            SetSource("println(\"hello\")");
        }

        public string IntermediateAssembly(string configuration) => Path.Combine(directory, "obj", configuration, "net10.0", $"{ProjectName}.dll");
        public string IntermediatePdb(string configuration) => Path.Combine(directory, "obj", configuration, "net10.0", $"{ProjectName}.pdb");
        public string OutputPdb(string configuration) => Path.Combine(directory, "bin", configuration, "net10.0", $"{ProjectName}.pdb");
        public string PathFromProject(string relativePath) => Path.GetFullPath(relativePath, directory);
        public string DescribePdbs() => string.Join(Environment.NewLine, Directory.GetFiles(directory, "*.pdb", SearchOption.AllDirectories)) + Environment.NewLine + lastBuildOutput;
        public void SetProperties(string properties, string items = "") => File.WriteAllText(
            Path.Combine(directory, $"{ProjectName}.csproj"),
            $"<Project Sdk=\"{SdkPackageId}/{SdkPackageVersion}\"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>{properties}</PropertyGroup>{items}</Project>");
        public void SetSource(string source) => File.WriteAllText(Path.Combine(directory, "main.b"), source);
        public void SetSource(string relativePath, string source)
        {
            var path = PathFromProject(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, source);
        }
        public void DeleteSource(string relativePath) => File.Delete(PathFromProject(relativePath));

        public void Build(string configuration, bool noRestore = false) => Run("build", configuration, noRestore);
        public void Clean(string configuration) => Run("clean", configuration, noRestore: false);
        public (int exitCode, string output) TryBuild(string configuration, bool noRestore = false) => RunProcess("build", configuration, noRestore);

        void Run(string command, string configuration, bool noRestore)
        {
            var result = RunProcess(command, configuration, noRestore);
            Assert.True(result.exitCode == 0, $"dotnet {command} failed.{Environment.NewLine}{result.output}");
        }

        (int exitCode, string output) RunProcess(string command, string configuration, bool noRestore)
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add(command);
            startInfo.ArgumentList.Add($"{ProjectName}.csproj");
            startInfo.ArgumentList.Add("--configuration");
            startInfo.ArgumentList.Add(configuration);
            startInfo.ArgumentList.Add("--verbosity");
            startInfo.ArgumentList.Add("normal");
            if (noRestore)
                startInfo.ArgumentList.Add("--no-restore");
            startInfo.Environment["NUGET_PACKAGES"] = packageCache;

            using var process = Process.Start(startInfo)!;
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(60_000))
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
                Task.WaitAll(outputTask, errorTask);
                Assert.Fail($"dotnet {command} timed out.");
            }
            Task.WaitAll(outputTask, errorTask);
            var output = outputTask.Result;
            var error = errorTask.Result;
            lastBuildOutput = output + Environment.NewLine + error;
            return (process.ExitCode, lastBuildOutput);
        }

        public void Dispose() => Directory.Delete(directory, recursive: true);
    }

    static string GetAssemblyMetadata(string key) => Assembly.GetExecutingAssembly()
                                                            .GetCustomAttributes<AssemblyMetadataAttribute>()
                                                            .Single(attribute => attribute.Key == key)
                                                            .Value!;
}
