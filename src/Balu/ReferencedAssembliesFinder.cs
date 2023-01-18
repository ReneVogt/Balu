using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

#pragma warning disable CA1031

namespace Balu;

internal static class ReferencedAssembliesFinder
{
    static List<string>? referencedAssemblies;

    public static string[] GetReferences()
    {
        if (referencedAssemblies is not null) return referencedAssemblies.ToArray();
        try
        {

            var startInfo = new ProcessStartInfo("dotnet", "--list-runtimes")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEnd();
            var netcore31Version = output.Split('\n')
                                    .Where(line => line.StartsWith("Microsoft.NETCore.App 6.0.", StringComparison.InvariantCulture))
                                    .Select(line => int.TryParse(line.Split(' ')[1][4..], out var version) ? version : -1)
                                    .DefaultIfEmpty(-1)
                                    .Max();
            if (netcore31Version < 0) return Array.Empty<string>();

            // C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.13\ref\net6.0
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                "dotnet", "packs", "Microsoft.NETCore.App.Ref",
                $"6.0.{netcore31Version}",
                "ref", "net6.0");
            var refs = new List<string>
            {
                Path.Combine(path, "System.Runtime.dll"),
                Path.Combine(path, "System.Runtime.Extensions.dll"),
                Path.Combine(path, "System.Console.dll")
            };
            Interlocked.CompareExchange(ref referencedAssemblies, refs, null);
            return referencedAssemblies.ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}