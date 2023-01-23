using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_Name_Reports_UndefinedName() => "var a = [bxy]".AssertScriptEvaluation("Undefined name 'bxy'.");
}