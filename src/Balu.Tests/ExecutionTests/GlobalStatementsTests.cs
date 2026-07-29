using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_GlobalStatement_ReportsInvalidExpressionStatement()
    {
        const string text = "42 * 17 function test() { [2*12] do {[17*12] }while false} do {[1+1]} while false";
        const string diagnostics = @"
            BL1026: Only assignment, call increment or decrement expressions can be used as a statement.
            BL1026: Only assignment, call increment or decrement expressions can be used as a statement.
            BL1026: Only assignment, call increment or decrement expressions can be used as a statement.
";
        text.AssertScriptEvaluation(diagnostics);
    }
    [Fact]
    public void Program_GlobalStatement_ReportsTwoMainMethods()
    {
        const string text1 = "function main() {}";
        const string text2 = "function [main]() {}";

        const string diagnostics = @"
           BL1020: Function 'main' is already declared.
";

        new[] { ("text1", text1), ("text2", text2) }.AssertProgramDiagnostics(diagnostics);
    }
    [Fact]
    public void Program_GlobalStatement_ReportsMisedMainAndGlobalInOneFile()
    {
        const string text1 = "[var a = 12] function [main](){}";

        const string diagnostics = @"
           BL1027: Global statements cannot be mixed with a 'main' function.
           BL1027: Global statements cannot be mixed with a 'main' function.
";

        new[] { ("text1", text1) }.AssertProgramDiagnostics(diagnostics);
    }
    [Fact]
    public void Program_GlobalStatement_ReportsMisedMainAndGlobalInTwoFiles()
    {
        const string text1 = "[var a = 12]";
        const string text2 = "function [main]() {}";

        const string diagnostics = @"
           BL1027: Global statements cannot be mixed with a 'main' function.
           BL1027: Global statements cannot be mixed with a 'main' function.
";

        new[] { ("text1", text1), ("text2", text2) }.AssertProgramDiagnostics(diagnostics);
    }
    [Fact]
    public void Program_GlobalStatement_ReportsMisedMainAndGlobalInTwoFiles2()
    {
        const string text1 = "function [main]() {}";
        const string text2 = "[var a = 12]";

        const string diagnostics = @"
           BL1027: Global statements cannot be mixed with a 'main' function.
           BL1027: Global statements cannot be mixed with a 'main' function.
";

        new[] { ("text1", text1), ("text2", text2) }.AssertProgramDiagnostics(diagnostics);
    }
    [Fact]
    public void Program_GlobalStatement_ReportsGlobalsInTwoFiles()
    {
        const string text1 = "[let b = true]";
        const string text2 = "[var a = 12]";

        const string diagnostics = @"
           BL1029: At most one file can contain global statements.
           BL1029: At most one file can contain global statements.
";

        new[] { ("text1", text1), ("text2", text2) }.AssertProgramDiagnostics(diagnostics);
    }
}
