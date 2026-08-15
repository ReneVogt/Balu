using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
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
        var (result, compilationDiagnostics, _) = RunGenerator(ValidSource);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var syntaxChildren = GetGeneratedSource(result, "SyntaxNodeChildren.g.cs");
        Assert.True(syntaxChildren.IndexOf("case 0: return First;", StringComparison.Ordinal) <
                    syntaxChildren.IndexOf("case 1: return Second;", StringComparison.Ordinal));

        var boundChildren = GetGeneratedSource(result, "BoundNodeChildren.g.cs");
        Assert.True(boundChildren.IndexOf("case 0: return Left;", StringComparison.Ordinal) <
                    boundChildren.IndexOf("case 1: return Right;", StringComparison.Ordinal));

        var rewriter = GetGeneratedSource(result, "BoundTreeRewriter.g.cs");
        Assert.Contains("new global::Balu.Binding.BoundPair(node.Syntax, rewrittenLeft, rewrittenRight)", rewriter);
    }

    [Fact]
    public void Generator_ReportsParameterWithoutMatchingProperty()
    {
        var (result, _, _) = RunGenerator(ValidSource.Replace("public SyntaxNode First { get; }", "public SyntaxNode Other { get; }", StringComparison.Ordinal));

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0001"));
        Assert.Contains("parameter 'first' has no property with the same name and type", diagnostic.GetMessage());
        Assert.True(diagnostic.Location.IsInSource);
    }

    [Fact]
    public void Generator_ReportsInaccessibleRewriterConstructor()
    {
        var source = ValidSource.Replace("public BoundPair(SyntaxNode syntax, BoundNode left, BoundNode right)",
                                         "private BoundPair(SyntaxNode syntax, BoundNode left, BoundNode right)",
                                         StringComparison.Ordinal);

        AssertInvalidNodeModel(source, "constructor with 3 parameters is not accessible from the generated bound tree rewriter");
    }

    [Fact]
    public void Generator_ReportsInaccessiblePropertyGetter()
    {
        var source = ValidSource.Replace("public BoundNode Left { get; }", "public BoundNode Left { private get; set; }", StringComparison.Ordinal);

        AssertInvalidNodeModel(source, "property 'Left' has no getter accessible from the generated bound tree rewriter");
    }

    [Fact]
    public void Generator_ReportsInheritedGetterInaccessibleFromGeneratedNodeMembers()
    {
        var source = ValidSource.Replace("public abstract int ChildrenCount { get; }",
                                         "protected SyntaxNode First { private get; set; } = null!;\n        public abstract int ChildrenCount { get; }",
                                         StringComparison.Ordinal)
                                .Replace("public SyntaxNode First { get; }",
                                         "public override int ChildrenCount => 0;\n        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));",
                                         StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0001"));
        Assert.Contains("property 'First' has no getter accessible from the generated syntax node members", diagnostic.GetMessage());
        Assert.DoesNotContain(compilationDiagnostics,
                              candidate => candidate.Severity == DiagnosticSeverity.Error && candidate.Id != "BLS0001");
    }

    [Fact]
    public void Generator_AllowsInaccessibleConstructorWhenRewriterDoesNotReconstructNode()
    {
        const string boundNode = """
    sealed partial class BoundLeaf : BoundNode
    {
        public int Value { get; }
        public override BoundNodeKind Kind => BoundNodeKind.Leaf;

        private BoundLeaf(SyntaxNode syntax, int value) : base(syntax)
        {
            Value = value;
        }
    }
""";
        var source = AddNodes(string.Empty, boundNode)
                    .Replace("enum BoundNodeKind { Pair }", "enum BoundNodeKind { Pair, Leaf }", StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains("VisitBoundLeaf", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_RejectsRecordNodes()
    {
        var (result, compilationDiagnostics, _) = RunGenerator(RecordSource);

        var diagnostics = result.Diagnostics.Where(candidate => candidate.Id == "BLS0002").ToImmutableArray();
        Assert.Equal(5, diagnostics.Length);
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("'Balu.Syntax.RecordSyntax'", StringComparison.Ordinal));
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("'Balu.Binding.BoundRecord'", StringComparison.Ordinal));
        Assert.All(diagnostics, diagnostic => Assert.Contains("must be a non-record, non-generic class", diagnostic.GetMessage()));
        Assert.DoesNotContain(compilationDiagnostics,
                              diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "BLS0002");
    }

    [Fact]
    public void Generator_EscapesKeywordPropertyIdentifiers()
    {
        var source = ValidSource.Replace("public SyntaxNode First { get; }", "public SyntaxNode @event { get; }", StringComparison.Ordinal)
                                .Replace("public OrderedSyntax(SyntaxNode first, SyntaxNode second)", "public OrderedSyntax(SyntaxNode @event, SyntaxNode second)", StringComparison.Ordinal)
                                .Replace("First = first;", "@event = @event;", StringComparison.Ordinal)
                                .Replace("public BoundNode Left { get; }", "public BoundNode @event { get; }", StringComparison.Ordinal)
                                .Replace("BoundNode left, BoundNode right", "BoundNode @event, BoundNode right", StringComparison.Ordinal)
                                .Replace("Left = left;", "@event = @event;", StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains("return @event;", GetGeneratedSource(result, "SyntaxNodeChildren.g.cs"));
        Assert.Contains("return @event;", GetGeneratedSource(result, "BoundNodeChildren.g.cs"));
        Assert.Contains("node.@event", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_EscapesKeywordKindIdentifiers()
    {
        var source = ValidSource.Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { @class }", StringComparison.Ordinal)
                                .Replace("OrderedSyntax", "classSyntax", StringComparison.Ordinal)
                                .Replace("SyntaxKind.Ordered", "SyntaxKind.@class", StringComparison.Ordinal)
                                .Replace("enum BoundNodeKind { Pair }", "enum BoundNodeKind { @event }", StringComparison.Ordinal)
                                .Replace("BoundPair", "Boundevent", StringComparison.Ordinal)
                                .Replace("BoundNodeKind.Pair", "BoundNodeKind.@event", StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains("case SyntaxKind.@class:", GetGeneratedSource(result, "SyntaxTreeVisitor.g.cs"));
        Assert.Contains("case BoundNodeKind.@event:", GetGeneratedSource(result, "BoundTreeVisitor.g.cs"));
        Assert.Contains("BoundNodeKind.@event =>", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_SupportsProductionStylePrimaryConstructors()
    {
        const string primaryConstructorNode = """
    sealed partial class BoundPair(SyntaxNode syntax, BoundNode left, BoundNode right) : BoundNode(syntax)
    {
        public BoundNode Right { get; } = right;
        public BoundNode Left { get; } = left;
        public override BoundNodeKind Kind => BoundNodeKind.Pair;
    }
""";
        var source = ValidSource.Replace(BoundPairSource, primaryConstructorNode, StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains("new global::Balu.Binding.BoundPair(node.Syntax, rewrittenLeft, rewrittenRight)", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_TraversesSingleSeparatedListWithSeparators()
    {
        const string syntaxNodes = """
    public sealed partial class SeparatedOnlySyntax : SyntaxNode
    {
        public SeparatedSyntaxList<SyntaxToken> Items { get; }
        public override SyntaxKind Kind => SyntaxKind.SeparatedOnly;

        public SeparatedOnlySyntax(SeparatedSyntaxList<SyntaxToken> items)
        {
            Items = items;
        }
    }

    public static class SeparatedOnlySyntaxProbe
    {
        public static int ChildrenCount => CreateNode().ChildrenCount;
        public static int GetChildId(int index) => ((SyntaxToken)CreateNode().GetChild(index)).Id;

        static SeparatedOnlySyntax CreateNode()
        {
            var children = ImmutableArray.Create<SyntaxNode>(new SyntaxToken(1), new SyntaxToken(2), new SyntaxToken(3));
            return new SeparatedOnlySyntax(new SeparatedSyntaxList<SyntaxToken>(children));
        }
    }
""";
        var source = AddNodes(syntaxNodes, string.Empty)
                    .Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { Ordered, SeparatedOnly }", StringComparison.Ordinal);
        var (result, compilationDiagnostics, outputCompilation) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        using var stream = new MemoryStream();
        var emitResult = outputCompilation.Emit(stream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));

        var assembly = Assembly.Load(stream.ToArray());
        var probe = assembly.GetType("Balu.Syntax.SeparatedOnlySyntaxProbe", throwOnError: true)!;
        var childrenCount = probe.GetProperty("ChildrenCount")!;
        var getChildId = probe.GetMethod("GetChildId")!;

        Assert.Equal(3, childrenCount.GetValue(null));
        Assert.Equal(1, InvokeGetChildId(0));
        Assert.Equal(2, InvokeGetChildId(1));
        Assert.Equal(3, InvokeGetChildId(2));
        Assert.IsType<IndexOutOfRangeException>(Assert.Throws<TargetInvocationException>(() => InvokeGetChildId(-1)).InnerException);
        Assert.IsType<IndexOutOfRangeException>(Assert.Throws<TargetInvocationException>(() => InvokeGetChildId(3)).InnerException);

        int InvokeGetChildId(int index) => (int)getChildId.Invoke(null, new object[] { index })!;
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
    public void Generator_RejectsFileLocalSyntaxAndBoundNodes()
    {
        const string syntaxNodes = """
    file sealed partial class FileSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Ordered;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
""";
        const string boundNodes = """
    file sealed partial class BoundFile : BoundNode
    {
        public override BoundNodeKind Kind => BoundNodeKind.Pair;
        public override int ChildrenCount => 0;
        public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
        public BoundFile(SyntaxNode syntax) : base(syntax) { }
    }
""";

        AssertUnsupportedNodeTypes(AddNodes(syntaxNodes, boundNodes),
                                   "Balu.Syntax.FileSyntax",
                                   "Balu.Binding.BoundFile");
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

    [Fact]
    public void Generator_SupportsNodesOutsideBaseNamespaces()
    {
        const string additionalSource = """

namespace Other.Syntax
{
    internal sealed partial class RemoteSyntax : Balu.Syntax.SyntaxNode
    {
        public override Balu.Syntax.SyntaxKind Kind => Balu.Syntax.SyntaxKind.Remote;
    }
}

namespace Other.Binding
{
    sealed partial class BoundRemote : Balu.Binding.BoundNode
    {
        public override Balu.Binding.BoundNodeKind Kind => Balu.Binding.BoundNodeKind.Remote;
        public BoundRemote(Balu.Syntax.SyntaxNode syntax) : base(syntax) { }
    }
}
""";
        var source = ValidSource.Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { Ordered, Remote }", StringComparison.Ordinal)
                                .Replace("enum BoundNodeKind { Pair }", "enum BoundNodeKind { Pair, Remote }", StringComparison.Ordinal) +
                     additionalSource;

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains("namespace Other.Syntax", GetGeneratedSource(result, "SyntaxNodeChildren.g.cs"));
        Assert.Contains("private protected virtual void VisitRemote(global::Other.Syntax.RemoteSyntax", GetGeneratedSource(result, "SyntaxTreeVisitor.g.cs"));
        Assert.Contains("namespace Other.Binding", GetGeneratedSource(result, "BoundNodeChildren.g.cs"));
        Assert.Contains("global::Other.Binding.BoundRemote", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_ReportsConcreteNodeWithoutMatchingKind()
    {
        const string syntaxNode = """
    public sealed class OrphanSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Ordered;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
""";

        var (result, _, _) = RunGenerator(AddNodes(syntaxNode, string.Empty));

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0003"));
        Assert.Contains("Balu.Syntax.OrphanSyntax", diagnostic.GetMessage());
        Assert.True(diagnostic.Location.IsInSource);
    }

    [Fact]
    public void Generator_ReportsKindWithoutMatchingNode()
    {
        var source = ValidSource.Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { Ordered, Missing }", StringComparison.Ordinal);

        var (result, _, _) = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0004"));
        Assert.Contains("SyntaxKind.Missing", diagnostic.GetMessage());
        Assert.Contains("MissingSyntax", diagnostic.GetMessage());
        Assert.True(diagnostic.Location.IsInSource);
    }

    [Fact]
    public void Generator_ReportsMultipleNodesForOneKindWithoutGeneratingCast()
    {
        const string duplicateNode = """

namespace Other.Syntax
{
    public sealed class OrderedSyntax : Balu.Syntax.SyntaxNode
    {
        public override Balu.Syntax.SyntaxKind Kind => Balu.Syntax.SyntaxKind.Ordered;
        public override int ChildrenCount => 0;
        public override Balu.Syntax.SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
}
""";

        var (result, _, _) = RunGenerator(ValidSource + duplicateNode);

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0005"));
        Assert.Contains("Balu.Syntax.OrderedSyntax", diagnostic.GetMessage());
        Assert.Contains("Other.Syntax.OrderedSyntax", diagnostic.GetMessage());
        Assert.DoesNotContain("case SyntaxKind.Ordered:", GetGeneratedSource(result, "SyntaxTreeVisitor.g.cs"));
    }

    [Fact]
    public void Generator_ReportsDetectableSyntaxAndBoundKindMismatchesWithoutGeneratingCasts()
    {
        const string syntaxNode = """
    public sealed partial class OtherSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Other;
    }
""";
        const string boundNode = """
    sealed partial class BoundOther : BoundNode
    {
        public override BoundNodeKind Kind => BoundNodeKind.Other;
        public BoundOther(SyntaxNode syntax) : base(syntax) { }
    }
""";
        var source = AddNodes(syntaxNode, boundNode)
                    .Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { Ordered, Other }", StringComparison.Ordinal)
                    .Replace("public override SyntaxKind Kind => SyntaxKind.Ordered;", "public override SyntaxKind Kind => (SyntaxKind)(1);", StringComparison.Ordinal)
                    .Replace("enum BoundNodeKind { Pair }", "enum BoundNodeKind { Pair, Other }", StringComparison.Ordinal)
                    .Replace("public override BoundNodeKind Kind => BoundNodeKind.Pair;",
                             "public override BoundNodeKind Kind { get { return BoundNodeKind.Other; } }",
                             StringComparison.Ordinal);

        var (result, _, _) = RunGenerator(source);

        var diagnostics = result.Diagnostics.Where(candidate => candidate.Id == "BLS0006").ToImmutableArray();
        Assert.Equal(2, diagnostics.Length);
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("OrderedSyntax", StringComparison.Ordinal));
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("BoundPair", StringComparison.Ordinal));
        Assert.DoesNotContain("case SyntaxKind.Ordered:", GetGeneratedSource(result, "SyntaxTreeVisitor.g.cs"));
        Assert.DoesNotContain("BoundNodeKind.Pair =>", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    [Fact]
    public void Generator_ReportsDuplicateKindValuesBeforeGeneratingSwitchCases()
    {
        const string syntaxNode = """
    public sealed partial class AliasSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Alias;
    }
""";
        var source = AddNodes(syntaxNode, string.Empty)
                    .Replace("public enum SyntaxKind { Ordered }", "public enum SyntaxKind { Ordered = 0, Alias = 0 }", StringComparison.Ordinal);

        var (result, _, _) = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0007"));
        Assert.Contains("SyntaxKind.Ordered", diagnostic.GetMessage());
        Assert.Contains("SyntaxKind.Alias", diagnostic.GetMessage());
        var visitor = GetGeneratedSource(result, "SyntaxTreeVisitor.g.cs");
        Assert.DoesNotContain("case SyntaxKind.Ordered:", visitor);
        Assert.DoesNotContain("case SyntaxKind.Alias:", visitor);
    }

    [Fact]
    public void Generator_ExemptsTokenKeywordAndTriviaKindsFromNodeMappings()
    {
        var source = ValidSource.Replace("public enum SyntaxKind { Ordered }",
                                         "public enum SyntaxKind { Ordered, BadToken, TrueKeyword, WhiteSpaceTrivia }",
                                         StringComparison.Ordinal);

        var (result, compilationDiagnostics, _) = RunGenerator(source);

        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(compilationDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    static void AssertUnsupportedNodeTypes(string source, params string[] typeNames)
    {
        var (result, compilationDiagnostics, _) = RunGenerator(source);
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

    static void AssertInvalidNodeModel(string source, string expectedReason)
    {
        var (result, compilationDiagnostics, _) = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics.Where(candidate => candidate.Id == "BLS0001"));
        Assert.Contains(expectedReason, diagnostic.GetMessage());
        Assert.True(diagnostic.Location.IsInSource);
        Assert.DoesNotContain(compilationDiagnostics,
                              candidate => candidate.Severity == DiagnosticSeverity.Error && candidate.Id != "BLS0001");
        Assert.DoesNotContain("VisitBoundPair", GetGeneratedSource(result, "BoundTreeRewriter.g.cs"));
    }

    static string AddNodes(string syntaxNodes, string boundNodes) =>
        ValidSource.Replace("    // Additional syntax nodes", syntaxNodes, StringComparison.Ordinal)
                   .Replace("    // Additional bound nodes", boundNodes, StringComparison.Ordinal);

    static (GeneratorDriverRunResult Result, ImmutableArray<Diagnostic> CompilationDiagnostics, CSharpCompilation OutputCompilation) RunGenerator(string source)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create("GeneratorTests",
                                                   new[] { syntaxTree },
                                                   GetReferences(),
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new BaluSourceGenerator() }, parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return (driver.GetRunResult(), outputCompilation.GetDiagnostics(), (CSharpCompilation)outputCompilation);
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
        public int Id { get; }
        public override SyntaxKind Kind => default;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));

        public SyntaxToken(int id = 0)
        {
            Id = id;
        }
    }

    public sealed class SeparatedSyntaxList<T> where T : SyntaxNode
    {
        public ImmutableArray<SyntaxNode> ElementsWithSeparators { get; }
        public T this[int index] => (T)ElementsWithSeparators[index * 2];

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> elementsWithSeparators)
        {
            ElementsWithSeparators = elementsWithSeparators;
        }
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

    const string BoundPairSource = """
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
""";

    const string RecordSource = """
using System;
using System.Collections.Immutable;

namespace Balu.Syntax
{
    public enum SyntaxKind { Record }

    public abstract record SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract int ChildrenCount { get; }
        public abstract SyntaxNode GetChild(int index);
    }

    public sealed record SyntaxToken : SyntaxNode
    {
        public override SyntaxKind Kind => default;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }

    public sealed class SeparatedSyntaxList<T> where T : SyntaxNode
    {
        public ImmutableArray<SyntaxNode> ElementsWithSeparators { get; } = ImmutableArray<SyntaxNode>.Empty;
    }

    public sealed partial record RecordSyntax : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Record;
        public override int ChildrenCount => 0;
        public override SyntaxNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
}

namespace Balu.Binding
{
    using Balu.Syntax;

    enum BoundNodeKind { Record }

    abstract record BoundNode(SyntaxNode Syntax)
    {
        public abstract BoundNodeKind Kind { get; }
        public abstract int ChildrenCount { get; }
        public abstract BoundNode GetChild(int index);
    }

    sealed partial record BoundRecord(SyntaxNode Syntax) : BoundNode(Syntax)
    {
        public override BoundNodeKind Kind => BoundNodeKind.Record;
        public override int ChildrenCount => 0;
        public override BoundNode GetChild(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    }
}
""";
}
