using System;
using System.Linq;
using Balu.Diagnostics;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.ParserTests;

public sealed class RecursionGuardTests
{
    [Fact]
    public void Parser_DeeplyNestedExpression_ReportsDiagnostic()
    {
        var source = new string('(', 300) + "1" + new string(')', 300);

        var syntaxTree = SyntaxTree.Parse(source);
        var compilation = Compilation.CreateScript(null, syntaxTree);

        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(syntaxTree.Diagnostics).Id);
        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(compilation.Diagnostics).Id);
        Assert.Empty(syntaxTree.Root.Members);
    }

    [Fact]
    public void Parser_DeeplyNestedStatements_ReportsDiagnostic()
    {
        var source = string.Concat(Enumerable.Repeat("if true ", 300)) + "print(1)";

        var syntaxTree = SyntaxTree.Parse(source);

        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(syntaxTree.Diagnostics).Id);
        Assert.Empty(syntaxTree.Root.Members);
    }

    [Fact]
    public void Compiler_DeepSyntaxTree_ReportsDiagnostic()
    {
        var source = string.Join(" + ", Enumerable.Repeat("value", 300));
        var syntaxTree = SyntaxTree.Parse(source);

        var compilation = Compilation.CreateScript(null, syntaxTree);

        Assert.Empty(syntaxTree.Diagnostics);
        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(compilation.Diagnostics).Id);
    }

    [Fact]
    public void Binder_RecursionFailure_DoesNotLeakNestedScope()
    {
        var expression = string.Join(" + ", Enumerable.Repeat("value", 300));
        var syntaxTree = SyntaxTree.Parse($"{{ var leaked = 0 {expression} }}");

        var compilation = Compilation.CreateScript(null, syntaxTree);

        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(compilation.Diagnostics).Id);
        Assert.DoesNotContain(compilation.VisibleSymbols, symbol => symbol.Name == "leaked");
    }

    [Fact]
    public void SyntaxTreeVisitor_DeepSyntaxTree_ThrowsCatchableException()
    {
        var source = string.Join(" + ", Enumerable.Repeat("1", 300));
        var syntaxTree = SyntaxTree.Parse(source);

        Assert.Throws<InsufficientExecutionStackException>(() => new DescendingVisitor().Visit(syntaxTree.Root));
        Assert.Throws<InsufficientExecutionStackException>(() => _ = syntaxTree.Root.GetChild(0).Parent);
    }

    sealed class DescendingVisitor : SyntaxTreeVisitor
    {
    }
}
