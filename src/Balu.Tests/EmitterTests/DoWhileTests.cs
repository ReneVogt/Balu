using Balu.Tests.TestHelper;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_DoWhile_BlockBody()
    {
        const string code = @"
            function test() [{]
                do
                [{]
                    [println("""")]
                [}] [while true]
            [}]
            return
";
        code.AssertSymbols("test");
    }
    [Fact]
    public void Emitter_DoWhile_SingleStatementBody()
    {
        const string code = @"
            function test() [{]
                do
                  [println("""")]
                [while 2 > 0]
            [}]
            return
";
        code.AssertSymbols("test");
    }
}