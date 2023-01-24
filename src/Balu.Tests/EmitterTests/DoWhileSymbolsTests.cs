using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_DoWhile_CorrectSequencePoints()
    {
        const string code = @"
            function test() {
                do
                {
                    [println("""")]
                } while [true]
            }
            return
";
        code.AssertSymbols("test");
    }
}