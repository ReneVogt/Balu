using System.Diagnostics;

#pragma warning disable IDE0079
#pragma warning disable CA1031

namespace Balu.Interpretation;

static class ReferencedAssembliesFinder
{
    static List<string>? referencedAssemblies;

    public static string[] GetReferences()
    {
        if (referencedAssemblies is not null) return [.. referencedAssemblies];
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
            var net9version = output.Split('\n')
                                    .Where(line => line.StartsWith("Microsoft.NETCore.App 9.0.", StringComparison.InvariantCulture))
                                    .Select(line => int.TryParse(line.Split(' ')[1][4..], out var version) ? version : -1)
                                    .DefaultIfEmpty(-1)
                                    .Max();
            if (net9version < 0) return [];

            // C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                "dotnet", "packs", "Microsoft.NETCore.App.Ref",
                $"9.0.{net9version}",
                "ref", "net9.0");
            var refs = new List<string>
            {
                Path.Combine(path, "System.Runtime.dll"),
                Path.Combine(path, "System.Runtime.Extensions.dll"),
                Path.Combine(path, "System.Console.dll")
            };
            Interlocked.CompareExchange(ref referencedAssemblies, refs, null);
            return [.. referencedAssemblies];
        }
        catch
        {
            return [];
        }
    }
}