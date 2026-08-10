using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Balu.SourceGenerator.Tests;

public sealed class GeneratorOrderingTests
{
    [Fact]
    public void Generator_UsesConstructorOrderForChildrenAndRewriterArguments()
    {
        var (result, compilationDiagnostics) = RunGenerator(ValidSource);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var syntaxChildren = GetGeneratedSource(result, "SyntaxNodeChildren.g.cs");
        Assert.True(syntaxChildren.IndexOf("case 0: return First;", StringComparison.Ordinal) <
                    syntaxChildren.IndexOf("case 1: return Second;", StringComparison.Ordinal));

        var boundChildren = GetGeneratedSource(result, "BoundNodeChildren.g.cs");
        Assert.True(boundChildren.IndexOf("case 0: return Left;", StringComparison.Ordinal) <
                    boundChildren.IndexOf("case 1: return Right;", StringComparison.Ordinal));

        var rewriter = GetGeneratedSource(result, "BoundTreeRewriter.g.cs");
        Assert.Contains("new BoundPair(node.Syntax, rewrittenLeft, rewrittenRight)", rewriter);
    }

    [Fact]
    public void Generator_ReportsParameterWithoutMatchingProperty()
    {
        var (result, _) = RunGenerator(ValidSource.Replace("public SyntaxNode First { get; }", "public SyntaxNode Other { get; }", StringComparison.Ordinal));

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0001"));
        Assert.Contains("parameter 'first' has no property with the same name and type", diagnostic.GetMessage());
        Assert.True(diagnostic.Location.IsInSource);
    }

    [Fact]
    public void Generator_RejectsNestedSyntaxAndBoundNodes()
    {
        const string syntaxNodes = """
    public partial class SyntaxContainer
    {
        public sealed partial class NestedSyntax : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Ordered;
            public override int ChildrenCount => 0;
            public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
""";
        const string boundNodes = """
    partial class BoundContainer
    {
        sealed partial class NestedBoundNode : BoundNode
        {
            public override BoundNodeKind Kind => BoundNodeKind.Pair;
            public override int ChildrenCount => 0;
            public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
            public NestedBoundNode(SyntaxNode syntax) : base(syntax) { }
        }
    }
""";

        AssertUnsupportedNodeTypes(AddNodes(syntaxNodes, boundNodes),
                                   "Balu.Syntax.SyntaxContainer.NestedSyntax",
                                   "Balu.Binding.BoundContainer.NestedBoundNode");
    }

    [Fact]
    public void Generator_RejectsGenericSyntaxAndBoundNodes()
    {
        const string syntaxNodes = """
    public sealed partial class GenericSyntax<T> : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Ordered;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
""";
        const string boundNodes = """
    sealed partial class GenericBoundNode<T> : BoundNode
    {
        public override BoundNodeKind Kind => BoundNodeKind.Pair;
        public override int ChildrenCount => 0;
        public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        public GenericBoundNode(SyntaxNode syntax) : base(syntax) { }
    }
""";

        AssertUnsupportedNodeTypes(AddNodes(syntaxNodes, boundNodes),
                                   "Balu.Syntax.GenericSyntax<T>",
                                   "Balu.Binding.GenericBoundNode<T>");
    }

    [Fact]
    public void Generator_RejectsNodesInGenericContainers()
    {
        const string syntaxNodes = """
    public partial class GenericContainer<T>
    {
        public sealed partial class NestedSyntax : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Ordered;
            public override int ChildrenCount => 0;
            public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
""";
        const string boundNodes = """
    partial class GenericContainer<T>
    {
        sealed partial class NestedBoundNode : BoundNode
        {
            public override BoundNodeKind Kind => BoundNodeKind.Pair;
            public override int ChildrenCount => 0;
            public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
            public NestedBoundNode(SyntaxNode syntax) : base(syntax) { }
        }
    }
""";

        AssertUnsupportedNodeTypes(AddNodes(syntaxNodes, boundNodes),
                                   "Balu.Syntax.GenericContainer<T>.NestedSyntax",
                                   "Balu.Binding.GenericContainer<T>.NestedBoundNode");
    }

    [Fact]
    public void Generator_RejectsDuplicateSimpleNamesWithoutCrashing()
    {
        const string syntaxNodes = """
    public partial class FirstContainer
    {
        public sealed partial class OrderedSyntax : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Ordered;
            public override int ChildrenCount => 0;
            public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public partial class SecondContainer
    {
        public sealed partial class OrderedSyntax : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Ordered;
            public override int ChildrenCount => 0;
            public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
""";
        const string boundNodes = """
    partial class FirstContainer
    {
        sealed partial class BoundPair : BoundNode
        {
            public override BoundNodeKind Kind => BoundNodeKind.Pair;
            public override int ChildrenCount => 0;
            public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
            public BoundPair(SyntaxNode syntax) : base(syntax) { }
        }
    }

    partial class SecondContainer
    {
        sealed partial class BoundPair : BoundNode
        {
            public override BoundNodeKind Kind => BoundNodeKind.Pair;
            public override int ChildrenCount => 0;
            public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
            public BoundPair(SyntaxNode syntax) : base(syntax) { }
        }
    }
""";
        var source = AddNodes(syntaxNodes, boundNodes);

        AssertUnsupportedNodeTypes(source,
                                   "Balu.Syntax.FirstContainer.OrderedSyntax",
                                   "Balu.Syntax.SecondContainer.OrderedSyntax",
                                   "Balu.Binding.FirstContainer.BoundPair",
                                   "Balu.Binding.SecondContainer.BoundPair");
    }

    static void AssertUnsupportedNodeTypes(string source, params string[] typeNames)
    {
        var (result, compilationDiagnostics) = RunGenerator(source);
        var generatorResult = Assert.Single(result.Results);

        Assert.Null(generatorResult.Exception);
        var diagnostics = result.Diagnostics.Where(candidate => candidate.Id == "BLS0002").ToImmutableArray();
        Assert.Equal(typeNames.Length, diagnostics.Length);
        foreach (var typeName in typeNames)
        {
            var diagnostic = Assert.Single(diagnostics.Where(candidate => candidate.GetMessage().Contains($"'{typeName}'", StringComparison.Ordinal)));
            Assert.True(diagnostic.Location.IsInSource);
        }
        Assert.DoesNotContain(compilationDiagnostics,
                              diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "BLS0002");
    }

    static string AddNodes(string syntaxNodes, string boundNodes) =>
        ValidSource.Replace("    // Additional syntax nodes", syntaxNodes, StringComparison.Ordinal)
                   .Replace("    // Additional bound nodes", boundNodes, StringComparison.Ordinal);

    static (GeneratorDriverRunResult Result, ImmutableArray<Diagnostic> CompilationDiagnostics) RunGenerator(string source)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create("GeneratorTests",
                                                   new[] { syntaxTree },
                                                   GetReferences(),
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new BaluSourceGenerator() }, parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return (driver.GetRunResult(), outputCompilation.GetDiagnostics());
    }

    static string GetGeneratedSource(GeneratorDriverRunResult result, string hintName) =>
        result.Results.Single().GeneratedSources.Single(source => source.HintName == hintName).SourceText.ToString();

    static IEnumerable<MetadataReference> GetReferences()
    {
        var trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ??
                                throw new InvalidOperationException("Trusted platform assemblies are unavailable.");
        return trustedAssemblies.Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
    }

    const string ValidSource = """
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Syntax
{
    public enum SyntaxKind { Ordered }

    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract int ChildrenCount { get; }
        public abstract SyntaxNode GetChild(int index);
    }

    public sealed class SyntaxToken : SyntaxNode
    {
        public override SyntaxKind Kind => default;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }

    sealed class SeparatedSyntaxList<T> where T : SyntaxNode
    {
        public ImmutableArray<SyntaxNode> ElementsWithSeparators { get; } = ImmutableArray<SyntaxNode>.Empty;
        public SyntaxNode this[int index] => ElementsWithSeparators[index];
    }

    public sealed partial class OrderedSyntax : SyntaxNode
    {
        public SyntaxNode Second { get; }
        public SyntaxNode First { get; }
        public override SyntaxKind Kind => SyntaxKind.Ordered;

        public OrderedSyntax(SyntaxNode first, SyntaxNode second)
        {
            First = first;
            Second = second;
        }

        public OrderedSyntax(SyntaxNode first) : this(first, first) { }

        public OrderedSyntax(string value, int count) : this(new SyntaxToken(), new SyntaxToken()) { }
    }

    // Additional syntax nodes
}

namespace Balu.Binding
{
    using Balu.Syntax;

    enum BoundNodeKind { Pair }

    abstract class BoundNode
    {
        protected BoundNode(SyntaxNode syntax) => Syntax = syntax;
        public SyntaxNode Syntax { get; }
        public abstract BoundNodeKind Kind { get; }
        public abstract int ChildrenCount { get; }
        public abstract BoundNode GetChild(int index);
    }

    abstract class BoundLoopStatement : BoundNode
    {
        protected BoundLoopStatement(SyntaxNode syntax) : base(syntax) { }
    }

    sealed partial class BoundPair : BoundNode
    {
        public BoundNode Right { get; }
        public BoundNode Left { get; }
        public override BoundNodeKind Kind => BoundNodeKind.Pair;

        public BoundPair(SyntaxNode syntax, BoundNode left, BoundNode right) : base(syntax)
        {
            Left = left;
            Right = right;
        }
    }

    // Additional bound nodes
}
""";
}
