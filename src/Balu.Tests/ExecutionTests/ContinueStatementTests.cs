using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.CompilationTests.ExecutionTests;

public partial class ExecutionTests
{
    [Fact]
    public void Script_Continue_ContinuesCorrectLoop()
    {
        @"
            var result = 0
            for i = 1 to 10
            {
                if i/2*2 == i continue
                for j = 11 to 15
                {
                   if j == 13 || j == 14 continue
                   result = result + i + j
                }
             }
             result            
        ".AssertScriptEvaluation(value: 265);

    }
}