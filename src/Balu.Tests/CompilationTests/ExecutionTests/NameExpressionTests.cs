using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Execute_Name_Reports_UndefinedName() => "var a = [bxy]".AssertEvaluation("Undefined name 'bxy'.");
}