using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_BlockStatement_NoInfiniteLoopIfClosedBraceMissing()
    {
        const string text = "{[)][]";
        var diagnostics = $@"
            Unexpected ClosedParenthesisToken (')'), expected IdentifierToken.
            Unexpected EndOfFileToken ('{'\0'}'), expected ClosedBraceToken.";
        text.AssertScriptEvaluation(diagnostics);
    }
}