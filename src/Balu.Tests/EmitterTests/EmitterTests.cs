using Xunit.Abstractions;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    readonly ITestOutputHelper? output;
    public EmitterTests(ITestOutputHelper? output)
    {
        this.output = output;
    }
}