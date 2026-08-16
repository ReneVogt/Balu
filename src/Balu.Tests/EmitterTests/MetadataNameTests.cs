using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using Mono.Cecil;
using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_ShadowedScriptFunctions_HaveUniqueDeterministicMetadataNames()
    {
        var first = Compilation.CreateScript(null, SyntaxTree.Parse("function value(): int { return 1 }"));
        var second = Compilation.CreateScript(first, SyntaxTree.Parse("function value(): int { return 2 }"));
        var compilation = Compilation.CreateScript(second, SyntaxTree.Parse("function value(): int { return 3 }"));

        var functions = compilation.AllSymbols
            .OfType<FunctionSymbol>()
            .Where(function => function.Name == "value")
            .ToArray();
        Assert.Equal(3, functions.Length);

        using var output = new MemoryStream();
        var result = compilation.Emit(
            "ShadowedFunctions",
            ReferenceProvider.References,
            output,
            null,
            ImmutableDictionary<GlobalVariableSymbol, object>.Empty);

        Assert.False(result.Diagnostics.HasErrors());
        var metadataNames = functions.Select(function => result.GlobalSymbolNames[function]).ToArray();
        Assert.Equal(new[] { "<value0>", "<value1>", "value" }, metadataNames);

        var image = output.ToArray();
        using (var assembly = AssemblyDefinition.ReadAssembly(new MemoryStream(image)))
        {
            var methods = assembly.MainModule.GetType("Program").Methods
                .Where(method => metadataNames.Contains(method.Name))
                .ToArray();

            Assert.Equal(metadataNames.OrderBy(name => name), methods.Select(method => method.Name).OrderBy(name => name));
            Assert.All(methods, method =>
            {
                Assert.Empty(method.Parameters);
                Assert.Equal("System.Int32", method.ReturnType.FullName);
            });
        }

        var loadContext = new AssemblyLoadContext("ShadowedFunctions", isCollectible: true);
        try
        {
            using var reflectionStream = new MemoryStream(image);
            var programType = loadContext.LoadFromStream(reflectionStream).GetType("Program")!;
            foreach (var metadataName in metadataNames)
            {
                var method = programType.GetMethod(metadataName, BindingFlags.Static | BindingFlags.NonPublic);
                Assert.NotNull(method);
                Assert.Empty(method.GetParameters());
                Assert.Equal(typeof(int), method.ReturnType);
            }
        }
        finally
        {
            loadContext.Unload();
        }
    }
}
