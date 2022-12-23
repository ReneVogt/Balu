using System;
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

        if (args.Length > 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Error: multiple source files are not yet supported.");
            Console.ResetColor();
            return;
        }

        try
        {
            var path = Path.GetFullPath(args.Single());
            var syntaxTree = SyntaxTree.Load(path);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new ());
            if (result.Diagnostics.Any())
                Console.Error.WriteDiagnostics(result.Diagnostics, compilation.SyntaxTree);
            else
                Console.WriteLine($"Result: {result.Value}");
        }
        catch (Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(error);
            Console.ResetColor();
        }
    }
}
