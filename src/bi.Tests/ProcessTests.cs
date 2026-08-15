using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace bi.Tests;

public sealed class ProcessTests
{
    static readonly string BiAssemblyPath = Assembly.GetExecutingAssembly()
                                                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                                                    .Single(attribute => attribute.Key == "BiAssemblyPath")
                                                    .Value!;

    [Fact]
    public async Task RedirectedInputExitsCleanly()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(BiAssemblyPath);

        using var process = Process.Start(startInfo)!;
        process.StandardInput.Close();
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        bool exited = process.WaitForExit(5_000);
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }

        string output = await outputTask;
        string error = await errorTask;

        Assert.True(exited, "bi did not exit when its input was redirected.");
        Assert.Equal(1, process.ExitCode);
        Assert.Empty(output);
        Assert.Equal($"Error: Interactive console input is unavailable.{Environment.NewLine}", error);
    }
}
