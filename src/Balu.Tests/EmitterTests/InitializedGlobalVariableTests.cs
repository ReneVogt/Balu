using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Theory]
    [InlineData("bool", "true", true)]
    [InlineData("int", "42", 42)]
    [InlineData("string", "\"text\"", "text")]
    [InlineData("any", "true", true)]
    [InlineData("any", "42", 42)]
    [InlineData("any", "\"text\"", "text")]
    public void Emitter_AcceptsValidInitializedGlobalVariable(string type, string literal, object value)
    {
        var compilation = CreateScriptWithGlobal(type, literal);
        var global = GetGlobal(compilation);
        var initializedGlobals = ImmutableDictionary<GlobalVariableSymbol, object>.Empty.Add(global, value);
        using var output = new MemoryStream();

        var result = compilation.Emit("ValidGlobals", ReferenceProvider.References, output, null, initializedGlobals);

        Assert.Empty(result.Diagnostics);
        Assert.NotEqual(0, output.Length);
    }

    [Theory]
    [InlineData("bool", "true", 1)]
    [InlineData("int", "42", false)]
    [InlineData("string", "\"text\"", 42)]
    [InlineData("any", "42", 1L)]
    public void Emitter_RejectsInvalidInitializedGlobalVariableType(string type, string literal, object value)
    {
        var compilation = CreateScriptWithGlobal(type, literal);
        var global = GetGlobal(compilation);
        var initializedGlobals = ImmutableDictionary<GlobalVariableSymbol, object>.Empty.Add(global, value);

        AssertInvalidInitializedGlobals(compilation, initializedGlobals);
    }

    [Fact]
    public void Emitter_RejectsNullInitializedGlobalVariable()
    {
        var compilation = CreateScriptWithGlobal("string", "\"text\"");
        var global = GetGlobal(compilation);
        var initializedGlobals = ImmutableDictionary<GlobalVariableSymbol, object>.Empty.Add(global, null!);

        AssertInvalidInitializedGlobals(compilation, initializedGlobals);
    }

    [Fact]
    public void Emitter_RejectsGlobalVariableFromUnrelatedCompilation()
    {
        var compilation = CreateScriptWithGlobal("int", "42");
        var unrelatedCompilation = CreateScriptWithGlobal("int", "42");
        var unrelatedGlobal = GetGlobal(unrelatedCompilation);
        var initializedGlobals = ImmutableDictionary<GlobalVariableSymbol, object>.Empty.Add(unrelatedGlobal, 42);

        AssertInvalidInitializedGlobals(compilation, initializedGlobals);
    }

    [Fact]
    public void Emitter_AcceptsShadowedGlobalVariableFromPreviousScriptCompilation()
    {
        var previous = CreateScriptWithGlobal("int", "42");
        var global = GetGlobal(previous);
        var compilation = Compilation.CreateScript(previous, SyntaxTree.Parse("var value: int = 0"));
        var initializedGlobals = ImmutableDictionary<GlobalVariableSymbol, object>.Empty.Add(global, 42);
        using var output = new MemoryStream();

        var result = compilation.Emit("InheritedGlobals", ReferenceProvider.References, output, null, initializedGlobals);

        Assert.False(result.Diagnostics.HasErrors());
        Assert.NotEqual(0, output.Length);
    }

    static Compilation CreateScriptWithGlobal(string type, string literal) =>
        Compilation.CreateScript(null, SyntaxTree.Parse($"var value: {type} = {literal}"));

    static GlobalVariableSymbol GetGlobal(Compilation compilation) =>
        compilation.VisibleSymbols.OfType<GlobalVariableSymbol>().Single(global => global.Name == "value");

    static void AssertInvalidInitializedGlobals(Compilation compilation, ImmutableDictionary<GlobalVariableSymbol, object> initializedGlobals)
    {
        using var output = new MemoryStream();
        output.WriteByte(42);
        var originalOutput = output.ToArray();

        var exception = Assert.Throws<ArgumentException>(() =>
            compilation.Emit("InvalidGlobals", ReferenceProvider.References, output, null, initializedGlobals));

        Assert.Equal("initializedGlobalVariables", exception.ParamName);
        Assert.Equal(originalOutput, output.ToArray());
    }
}
