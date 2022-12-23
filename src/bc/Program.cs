using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balu;
using Balu.Syntax;
using Balu.Visualization;

#pragma warning disable CA1031 // Main catches all exceptions

sealed class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Usage: bc <sourefilepath>");
            Console.ResetColor();
            return;
        }

        try
        {
            var syntaxTrees = GetFilePaths(args).Select(path => SyntaxTree.Load(Path.GetFullPath(path))).ToArray();
            var compilation = new Compilation(syntaxTrees);
            var result = compilation.Evaluate(new ());
            if (result.Diagnostics.Any())
                Console.Error.WriteDiagnostics(result.Diagnostics);
            else
            {
                var output = result.Value is string s ? $"\"{s.EscapeString()}\"" : result.Value;
                Console.WriteLine($"Result: {output}");
            }
        }
        catch (Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(error);
            Console.ResetColor();
        }
    }

    static IEnumerable<string> GetFilePaths(IEnumerable<string> args)
    {
        var result = new SortedSet<string>();
        foreach (var path in args)
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
                result.UnionWith(Directory.EnumerateFiles(path, "*.balu", SearchOption.AllDirectories));
            else
                result.Add(path);
        }

        return result;
    }
}
