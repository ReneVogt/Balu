namespace Balu.Tests.TestHelper;

static class ReferenceProvider
{
    // HACK: find a way to get this for tests
    public static string[] References { get; } =
    [
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.Extensions.dll",
        @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Console.dll"
    ];
}
