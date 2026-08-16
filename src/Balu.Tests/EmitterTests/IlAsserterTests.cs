using TestHelpers;
using Xunit;
using Xunit.Sdk;

namespace Balu.Tests.EmitterTests;

public class IlAsserterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SymbolAssertions_Fail_When_SequencePoint_Count_Does_Not_Match(bool tooManyOffsets)
    {
        var code = tooManyOffsets ? "[1]" : "[1] + [2]";
        var offsets = tooManyOffsets ? new[] { 0, 1 } : new[] { 0 };
        var spanCount = tooManyOffsets ? 1 : 2;
        var expectedMessage = $"Annotated span count ({spanCount}) does not match supplied sequence-point offset count ({offsets.Length}).";

        var symbolsException = Assert.ThrowsAny<XunitException>(() => code.AssertSymbols("main", offsets));
        Assert.Contains(expectedMessage, symbolsException.Message);

        var combinedException = Assert.ThrowsAny<XunitException>(() => code.AssertIlAndSymbols("main", "", offsets));
        Assert.Contains(expectedMessage, combinedException.Message);
    }
}
