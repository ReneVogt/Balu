using TestHelpers;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_Name_Reports_UndefinedName() => "var a = [bxy]".AssertScriptEvaluation("BL1005: Undefined name 'bxy'.");
}
