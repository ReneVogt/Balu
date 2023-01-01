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
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Usage: bc <sourefilepath>");
            Console.ResetColor();
            return 1;
        }

        try
        {
            var syntaxTrees = GetFilePaths(args).Select(path => SyntaxTree.Load(Path.GetFullPath(path))).ToArray();
            var compilation = Compilation.Create(syntaxTrees);
            var result = compilation.Evaluate(new ());
            if (result.Diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
                return 1;
            }

            var output = result.Value is string s ? $"\"{s.EscapeString()}\"" : result.Value;
            Console.WriteLine($"Result: {output}");
            return 0;
        }
        catch (Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(error);
            Console.ResetColor();
            return 1;
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
