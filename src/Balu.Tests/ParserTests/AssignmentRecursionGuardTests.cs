using System.Linq;
using Balu.Diagnostics;
using Balu.Syntax;
using Xunit;

namespace Balu.Tests.ParserTests;

public sealed class AssignmentRecursionGuardTests
{
    [Fact]
    public void Parser_DeeplyNestedAssignments_ReportDiagnostic()
    {
        var source = string.Concat(Enumerable.Range(0, 300).Select(index => $"x{index} = ")) + "1";

        var syntaxTree = SyntaxTree.Parse(source);

        Assert.Equal(DiagnosticId.InsufficientExecutionStack, Assert.Single(syntaxTree.Diagnostics).Id);
        Assert.Empty(syntaxTree.Root.Members);
    }
}
